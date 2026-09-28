namespace Common.Caching;

/// <summary>
/// Cached representation of a user's role: primary key and canonical string name.
/// </summary>
public record CachedUserRole(byte RolePK, string RoleName);
