# Role Authorization Migration: Design Notes

## Overview

This document describes the migration from string-based role constants to a strongly-typed C# `Role` enum. The migration is intentionally scoped to the C# application layer only, with zero database schema changes or JWT token format changes.

**Date:** 2024
**Target:** .NET 10
**Branch:** feature/jwt-authentication
**Status:** Complete

---

## Design Decision: C#-Layer Enum Without Database Migration

### Why Not a Full Database Migration?

The project is currently in **MVP phase** with deliberately deferred security features:
- Passwords stored in plaintext (no hashing)
- No refresh tokens
- No rate limiting
- Basic JWT validation only

A full database migration (changing `RoleTypes` table PK from GUID to TINYINT) would introduce:
- Complex data migration logic
- Potential data loss or corruption vectors
- Coordination requirements with all API clients
- Breaking changes to API contracts and JWT token format
- Testing burden for edge cases

**Conclusion:** A C#-only enum provides immediate benefits (type safety, compile-time validation, hierarchy clarity) **without the deployment risk** of a schema migration. The enum structure is now proven and documented; if/when the project graduates from MVP, this enum design can guide a full database migration.

---

## Architecture: Role Enum and Conversion Boundary

### The Role Enum (Internal Implementation Detail)

```csharp
public enum Role : byte
{
	User = 1,           // Lowest privilege
	Manager = 2,        // Middle privilege
	Administrator = 3   // Highest privilege
}
```

**Key Points:**
- Numeric values represent privilege levels; **higher number = higher privilege**
- These values are an **internal C# implementation detail only**
- Never exposed directly to clients, database, or JWT tokens

### String Representations (External Contract)

Canonical role strings used at all system boundaries:
- `"Administrator"` (matches `RoleNames.Admin` constant)
- `"Manager"` (matches `RoleNames.Manager` constant)
- `"User"` (matches `RoleNames.User` constant)

These strings:
- Are stored in the database (RoleTypes.RoleName column)
- Are embedded in JWT claims (ClaimTypes.Role = "Administrator")
- Are returned in API responses (UserDTO.RoleName as JSON string)
- **Must never change** — they form backward-compatible contracts with consumers

### Conversion Boundary: RoleExtensions

**Single-responsibility principle:** All enum ↔ string conversions happen in `Common/Auth/RoleExtensions.cs`, nowhere else.

**Method 1: `TryParseRole(string? roleName, out Role role)` — Non-Throwing**
```csharp
// For untrusted input (JWT claims, user data, database hydration)
if (RoleExtensions.TryParseRole(claimValue, out var role))
{
	// Use role safely
}
else
{
	_logger.LogWarning("Invalid role...");
	context.Fail();
}
```

- Takes a string, returns bool + out parameter
- Never throws
- Case-insensitive matching (OrdinalIgnoreCase)
- Used in authorization handler, parsers, validation logic

**Method 2: `ParseRole(string? roleName)` — Throwing**
```csharp
// For trusted input (seed data, config, hardcoded values)
var role = RoleExtensions.ParseRole("Administrator");
```

- Takes a string, returns Role or throws
- Throws InvalidOperationException with clear message
- Used in initialization, trusted data loading

**Method 3: `ToDisplayString(this Role role)` — Extension Method**
```csharp
// Converts enum back to canonical string
string roleString = Role.Administrator.ToDisplayString(); // Returns "Administrator"
```

- Extension method on Role enum
- Deterministic — must never throw
- Used everywhere an enum needs to be serialized

---

## Hierarchy Comparison: Old vs. New

### Old System (Dictionary-Based)

```csharp
private static readonly Dictionary<string, int> Levels = new()
{
	[RoleNames.Admin] = 2,      // Admin level = 2
	[RoleNames.Manager] = 1,    // Manager level = 1
	[RoleNames.User] = 0        // User level = 0
};

public static bool IsAtLeast(string? role, string? minimumRole) =>
	LevelOf(role) >= LevelOf(minimumRole);
```

**Problem:** String-based, error-prone, `-1` return value for unknown roles.

### New System (Enum-Based)

```csharp
public enum Role : byte
{
	User = 1,           // User level = 1
	Manager = 2,        // Manager level = 2
	Administrator = 3   // Admin level = 3
}

public static bool IsAtLeast(Role requesterRole, Role minimumRole) =>
	(int)requesterRole >= (int)minimumRole;
```

**Benefits:**
- Type-safe — compiler catches typos in role names
- Direct integer comparison — faster, clearer intent
- No dictionary lookups or error cases
- Hierarchy is obvious from enum values

### Hierarchy Mapping Reference

| Role | Old Level | New Enum Value |
|------|-----------|-----------------|
| User | 0 | 1 |
| Manager | 1 | 2 |
| Administrator | 2 | 3 |

**Privilege ordering is preserved:** Administrator > Manager > User

---

## Authorization Handler Changes

### Before (String-Based)

```csharp
var requesterRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

bool allowed = requirement.Operation switch
{
	UserOperation.View => RoleHierarchy.IsAtLeast(requesterRole, target.RoleName),
	UserOperation.Edit =>
		requesterRole == RoleNames.Admin
		|| (requesterRole == RoleNames.Manager && target.RoleName == RoleNames.User)
		|| (requesterRole == RoleNames.User && requesterId == target.UserPK),
	UserOperation.Delete =>
		requesterRole == RoleNames.Admin
		|| (requesterRole == RoleNames.Manager && target.RoleName == RoleNames.User),
};
```

**Issues:**
- Role claims never validated — invalid strings silently allowed or denied
- String comparisons against hardcoded constants — fragile
- Dictionary lookups in hierarchy check — potential for null/unknown role errors
- No logging of authorization decisions

### After (Enum-Based)

```csharp
// Parse and validate requester role from JWT claim
if (!RoleExtensions.TryParseRole(requesterRoleString, out var requesterRole))
{
	_logger.LogWarning("Invalid role '{InvalidRole}' in JWT claim. Access denied.", requesterRoleString);
	return Task.CompletedTask;
}

// Parse and validate target role from database
if (!RoleExtensions.TryParseRole(target.RoleName, out var targetRole))
{
	_logger.LogWarning("Target user has invalid role '{InvalidRole}' in database. Access denied.", target.RoleName);
	return Task.CompletedTask;
}

bool allowed = requirement.Operation switch
{
	UserOperation.View => RoleHierarchy.IsAtLeast(requesterRole, targetRole),
	UserOperation.Edit =>
		requesterRole == Role.Administrator
		|| (requesterRole == Role.Manager && targetRole == Role.User)
		|| (requesterRole == Role.User && requesterId == target.UserPK),
	UserOperation.Delete =>
		requesterRole == Role.Administrator
		|| (requesterRole == Role.Manager && targetRole == Role.User),
};

if (allowed)
{
	_logger.LogInformation("Authorization granted: {Operation} by {RequesterRole}", requirement.Operation, requesterRole);
	context.Succeed(requirement);
}
```

**Improvements:**
- Invalid role claims are caught and logged (not silently allowed)
- Role comparisons are type-safe and compile-checked
- Hierarchy comparisons are direct and efficient
- Authorization decisions are audited

---

## JWT Tokens: No Changes Needed

Current token generation in `Services/Auth/AuthService.cs`:

```csharp
var claims = new List<Claim>
{
	new Claim(ClaimTypes.NameIdentifier, user.UserPK.ToString()),
	new Claim(ClaimTypes.Name, user.UserName),
	new Claim(ClaimTypes.Role, user.RoleName)  // user.RoleName is a string like "Administrator"
};

var token = _jwtTokenService.GenerateToken(claims);
```

**No changes made:**
- Role claim still carries the string value (e.g., `"Administrator"`)
- Token format unchanged
- Existing tokens remain valid
- Claims extraction unchanged

**How it works now:**
1. JWT token is issued with `ClaimTypes.Role = "Administrator"` (string)
2. Token is validated by middleware
3. UserAccessHandler extracts the role string: `context.User.FindFirst(ClaimTypes.Role)?.Value`
4. UserAccessHandler converts string to enum: `RoleExtensions.TryParseRole(..., out var role)`
5. Authorization checks proceed with enum

---

## Database Schema: No Changes Needed

Current schema:

```sql
CREATE TABLE RoleTypes (
	RoleTypePK UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	RoleName VARCHAR(50) NOT NULL
);

CREATE TABLE Users (
	UserPK UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	UserName NVARCHAR(50) NOT NULL,
	Email NVARCHAR(50) NOT NULL,
	Password VARCHAR(50) NOT NULL,
	ActiveStatus BIT NOT NULL,
	RoleTypePK UNIQUEIDENTIFIER NOT NULL,
	CONSTRAINT FK_Users_RoleTypes FOREIGN KEY (RoleTypePK) REFERENCES RoleTypes(RoleTypePK)
);
```

**No changes:**
- RoleTypes table remains as-is with GUID PK and VARCHAR role names
- Users.RoleTypePK foreign key unchanged
- Seed data unchanged: rows with "Administrator", "Manager", "User"
- All queries unchanged

**Queries continue to work:**
```sql
SELECT u.UserName, u.Email, rt.RoleName
FROM Users u
JOIN RoleTypes rt ON u.RoleTypePK = rt.RoleTypePK
```

---

## API Contracts: No Breaking Changes

### Request/Response Serialization

Role properties in DTOs continue to serialize as strings:

```json
{
  "userPK": "550e8400-e29b-41d4-a716-446655440000",
  "userName": "john.doe",
  "email": "john@example.com",
  "roleName": "Administrator",
  "activeStatus": true,
  "firstName": "John",
  "secondName": "Doe",
  "birthDate": "1990-01-15"
}
```

**No changes:**
- `roleName` field is a string, not a numeric enum value
- JSON is identical to pre-migration format
- API clients see no difference
- Clients expecting string role names continue to work

---

## Unit Test Strategy

Eight unit tests validate the enum-to-string mapping and hierarchy:

1. **ToDisplayString Correctness**: Each enum value maps to exactly one canonical string
   - Guards against accidental enum renames breaking production
   - Test is **critical** — any rename of an enum value fails CI immediately

2. **TryParseRole Success Cases**: All three canonical strings parse correctly

3. **TryParseRole Failure Cases**: Invalid input is rejected without throwing

4. **TryParseRole Case Insensitivity**: Parsing is case-insensitive (matches existing behavior)

5. **ParseRole Throws on Invalid**: Throwing version works as expected

6. **RoleHierarchy.IsAtLeast**: Privilege comparisons work correctly
   - Administrator >= Manager >= User
   - Reverse comparisons are false
   - Equal privileges return true

7. **TargetUserInfo.RoleEnum Property**: Parses correctly from RoleName string

8. **Authorization Handler Integration**: (Pseudo-test demonstrating concept)

---

## Future Roadmap: Full Database Migration (if MVP ends)

When the project graduates from MVP and coordinates with API clients, a **full database schema migration** can proceed:

1. **Expand Phase:** Add new `Users.RoleId TINYINT` column alongside RoleTypePK
2. **Backfill Phase:** Populate RoleId with enum values (1, 2, 3)
3. **Dual-Write Phase:** Code writes to both RoleTypePK and RoleId for 1+ week
4. **Read-Cutover Phase:** Queries switch to reading from RoleId instead of RoleTypePK
5. **Cleanup Phase:** Drop RoleTypePK column and RoleTypes table after monitoring period

The `Role` enum structure defined here would guide all these steps.

---

## Code Style and Naming Conventions

### Role Enum Values

- All values are **Pascal case** (e.g., `Administrator`, `Manager`, `User`)
- Numeric values reflect privilege levels: higher = more privileged
- XML documentation explains each value

### RoleExtensions Methods

- `TryParseRole` follows the `Try*` pattern: returns `bool`, accepts `out` parameter, never throws
- `ParseRole` is the throwing variant: used only for trusted input
- `ToDisplayString` is an extension method: converts enum to string for output
- All methods include comprehensive XML documentation with usage examples

### Authorization Handler

- Logs invalid role claims (for audit trail)
- Logs authorization decisions at INFO level
- Uses descriptive log messages with role and user ID

### Tests

- Test method names follow Arrange-Act-Assert (AAA) pattern
- Each test validates ONE specific behavior
- Comments explain the business rule being verified
- Test names clearly state the expected outcome

---

## Adding New Roles in the Future

If a new role is added (e.g., `Supervisor`), the following steps must be done atomically:

1. Add new enum value: `Supervisor = 2` (or appropriate level)
2. Update `RoleExtensions.TryParseRole()` to handle "Supervisor" string
3. Update `RoleExtensions.ParseRole()` error message to list all roles
4. Update `RoleExtensions.ToDisplayString()` switch statement
5. Add unit test asserting `Role.Supervisor.ToDisplayString() == "Supervisor"`
6. Update `RoleHierarchy.IsAtLeast()` if the new role affects privilege ordering
7. Update `UserAccessHandler` permission rules if needed
8. Update database seed data with new role
9. Add migration documentation
10. Deploy atomically; do not separate these changes across multiple commits/deployments

---

## Rollback Plan

To revert to string-only role handling (if needed):

1. Remove `Common/Auth/Role.cs`
2. Remove `Common/Auth/RoleExtensions.cs`
3. Revert `RoleHierarchy.cs` to string-based Dictionary implementation
4. Revert `UserAccessHandler.cs` to string-based role comparisons
5. Revert `UserAccessRequirement.cs` to remove RoleEnum property
6. Restore original `Common/Constants/RoleNames.cs` usage
7. Remove `ROLE_MIGRATION_NOTES.md`

**Estimated Time:** ~2 hours

**Data Loss Risk:** None — no database or JWT token changes were made

---

## Summary

The Role enum migration provides **immediate type-safety benefits** without the coordination burden or breaking changes of a full database migration. The design preserves backward compatibility with existing clients, JWT tokens, and database schema while laying groundwork for future schema changes if the project evolves beyond MVP.

**Key Success Metric:** All authorization decisions use the strongly-typed Role enum; no new string-based role comparisons are added to the codebase.
