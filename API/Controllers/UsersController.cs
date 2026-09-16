using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Common.Auth;
using Common.Constants;
using Common.DTOs;
using Common.Interfaces;

namespace API.Controllers;

/// <summary>
/// API controller exposing user-related endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase {
    private readonly IUsersService _service;
    private readonly ITokenClaimsService _tokenClaimsService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Creates a new <see cref="UsersController"/>.
    /// </summary>
    /// <param name="service">Service providing user operations.</param>
    /// <param name="tokenClaimsService">Service for extracting claims from the authenticated user.</param>
    /// <param name="authorizationService">Service for authorization checks.</param>
    public UsersController(
        IUsersService service,
        ITokenClaimsService tokenClaimsService,
        IAuthorizationService authorizationService) {
        _service = service;
        _tokenClaimsService = tokenClaimsService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Get a user by primary key.
    /// </summary>
    /// <param name="id">User primary key.</param>
    /// <returns>200 with user DTO or 404 if not found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByPK(Guid id) {
        var dto = await _service.GetByPKAsync(id);

        return dto is null
            ? NotFound()
            : Ok(dto);
    }

    /// <summary>
    /// Get a paginated list of users.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByPage(
        int currentPage,
        int pageSize,
        string? sortExpression,
        string? searchValue,
        bool searchByUserName,
        bool searchByEmail,
        bool searchByFirstName,
        bool searchBySecondName,
        bool includeInactive,
        bool strictMatch) {
        var requestingUserRole = _tokenClaimsService.GetRoleFromPrincipal(User);

        if (string.IsNullOrWhiteSpace(requestingUserRole)) {
            return Forbid();
        }

        var (items, totalRows) = await _service.GetByPageAsync(
            requestingUserRole,
            currentPage,
            pageSize,
            sortExpression,
            searchValue,
            searchByUserName,
            searchByEmail,
            searchByFirstName,
            searchBySecondName,
            includeInactive,
            strictMatch);

        return Ok(new { items, totalRows });
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    /// <param name="dto">Write DTO containing user data.</param>
    /// <returns>201 Created with the created user.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] UserCreateUpdateDTO dto) {
        var created = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetByPK),
            new { id = created.UserPK },
            created);
    }

    /// <summary>
    /// Update an existing user.
    /// </summary>
    /// <param name="dto">
    /// DTO containing the primary key of the user to update and the new values.
    /// </param>
    /// <returns>204 if updated or 404 if the user does not exist.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromBody] UserCreateUpdateDTO dto) {
        var target = await _service.GetByPKAsync(dto.UserPK);

        if (target is null) {
            return NotFound();
        }

        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            new TargetUserInfo(target.UserPK, target.RoleName),
            new UserAccessRequirement(UserOperation.Edit));

        if (!authResult.Succeeded) {
            return Forbid();
        }

        var updated = await _service.UpdateAsync(dto);

        return updated
            ? NoContent()
            : NotFound();
    }

    /// <summary>
    /// Delete the user identified by the specified primary key.
    /// </summary>
    /// <param name="id">User primary key.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id) {
        var target = await _service.GetByPKAsync(id);

        if (target is null) {
            return NotFound();
        }

        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            new TargetUserInfo(target.UserPK, target.RoleName),
            new UserAccessRequirement(UserOperation.Delete));

        if (!authResult.Succeeded) {
            return Forbid();
        }

        await _service.DeleteAsync(id);

        return NoContent();
    }
}
