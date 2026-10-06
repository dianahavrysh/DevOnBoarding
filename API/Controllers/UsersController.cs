using System;
using System.Threading.Tasks;
using API.Attributes;
using Common.DTOs;
using Common.Enums;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase {
    private readonly IUsersService _service;

    public UsersController(IUsersService service) {
        _service = service;
    }

    [HttpGet("{id}")]
    [UserAccess(UserOperation.View)]
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
        var (items, totalRows) = await _service.GetByPageAsync(
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
