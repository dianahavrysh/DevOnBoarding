using System;
using System.Threading.Tasks;
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
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByPK(Guid id) {
        var result = await _service.GetByPKAsync(id);

        return result.Status switch {
            OperationStatus.Success => Ok(result.Value),
            OperationStatus.NotFound => NotFound(),
            _ => Forbid()
        };
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
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] UserCreateUpdateDTO dto) {
        var result = await _service.CreateAsync(dto);

        return result.Status switch {
            OperationStatus.Success => CreatedAtAction(
                nameof(GetByPK), new { id = result.Value!.UserPK }, result.Value),
            OperationStatus.NotFound => Problem(statusCode: StatusCodes.Status500InternalServerError),
            _ => Forbid()
        };
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromBody] UserCreateUpdateDTO dto) =>
        ToActionResult(await _service.UpdateAsync(dto));

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id) =>
        ToActionResult(await _service.DeleteAsync(id));

    private IActionResult ToActionResult(OperationStatus status) => status switch {
        OperationStatus.Success => NoContent(),
        OperationStatus.NotFound => NotFound(),
        _ => Forbid()
    };
}
