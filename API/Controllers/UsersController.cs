using Common.DTOs;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers {
    /// <summary>
    /// API controller exposing user-related endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase {
        private readonly IUsersService _service;

        /// <summary>
        /// Creates a new <see cref="UsersController"/>.
        /// </summary>
        /// <param name="service">Service providing user operations.</param>
        public UsersController(IUsersService service) {
            _service = service;
        }

        /// <summary>
        /// Get a user by primary key.
        /// </summary>
        /// <param name="id">User primary key.</param>
        /// <returns>200 with user DTO or 404 if not found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByPK(Guid id) {
            var dto = await _service.GetByPKAsync(id);

            if (dto == null) {
                return NotFound();
            }

            return Ok(dto);
        }

        /// <summary>
        /// Get a paginated list of users.
        /// </summary>
        // TODO: requestingUserPK should come from the authenticated user's claims
        // once the login/session subsystem exists, not from the query string.
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPage(
            [FromQuery] Guid requestingUserPK,
            [FromQuery] int currentPage = 1,
            [FromQuery] int pageSize = 20) {

            var (items, total) = await _service.GetByPageAsync(
                requestingUserPK,
                currentPage,
                pageSize,
                null,
                null,
                null,
                false,
                false);

            return Ok(new {
                TotalRows = total,
                Items = items
            });
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="dto">Write DTO containing user data.</param>
        /// <returns>201 Created with the created user.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        public async Task<IActionResult> Update([FromBody] UserCreateUpdateDTO dto) {
            var updated = await _service.UpdateAsync(dto);

            if (!updated) {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Delete the user identified by the specified primary key.
        /// </summary>
        /// <param name="id">User primary key.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id) {
            await _service.DeleteAsync(id);

            return NoContent();
        }
    }
}
