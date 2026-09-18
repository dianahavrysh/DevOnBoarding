using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Common.Enums;
using Common.DTOs;
using Common.Interfaces;
using API.Attributes;

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

    /// <summary>
    /// Creates a new <see cref="UsersController"/>.
    /// </summary>
    public UsersController(
        IUsersService service,
        ITokenClaimsService tokenClaimsService) {
        _service = service;
        _tokenClaimsService = tokenClaimsService;
    }

    /// <summary>
    /// Get a user by primary key.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        var requestingUserRole =
            _tokenClaimsService.GetRoleFromPrincipal(User);

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
    [HttpPost]
    [UserAccess(UserOperation.Create)]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] UserCreateUpdateDTO dto) {
        var created = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetByPK),
            new { id = created.UserPK },
            created);
    }

    /// <summary>
    /// Update an existing user.
    /// </summary>
    [HttpPut]
    [UserAccess(UserOperation.Edit)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        [FromBody] UserCreateUpdateDTO dto) {
        var updated = await _service.UpdateAsync(dto);

        return updated
            ? NoContent()
            : NotFound();
    }

    /// <summary>
    /// Delete the user identified by the specified primary key.
    /// </summary>
    [HttpDelete("{id}")]
    [UserAccess(UserOperation.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id) {
        await _service.DeleteAsync(id);

        return NoContent();
    }
}
