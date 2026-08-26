using Common.DTOs;
using Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    /// <summary>
    /// API controller exposing user-related endpoints.
    /// </summary>
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Service providing user-related operations.
        /// </summary>
        private readonly IUsersService _service;

        /// <summary>
        /// Creates a new <see cref="UsersController"/>.
        /// </summary>
        /// <param name="service">Service providing user operations.</param>
        public UsersController(IUsersService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get a user by primary key.
        /// </summary>
        /// <param name="id">User primary key.</param>
        /// <returns>200 with user DTO or 404 if not found.</returns>
        [HttpGet("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> GetByPK(Guid id)
        {
            var dto = await _service.GetByPKAsync(id).ConfigureAwait(false);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Get a paginated list of users.
        /// </summary>
        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetByPage(
        [FromQuery] Guid requestingUserPK,
        [FromQuery] int currentPage = 1,
        [FromQuery] int pageSize = 20) {
            var (items, total) = await _service.GetByPageAsync(requestingUserPK, currentPage, pageSize, null, null, null, false, false).ConfigureAwait(false);
            return Ok(new { TotalRows = total, Items = items });
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="dto">Write DTO containing user data.</param>
        /// <returns>201 Created with Location header to the new resource.</returns>
        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> Create([FromBody] UserCreateUpdateDTO dto)
        {
            var id = await _service.CreateAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetByPK), new { id }, null);
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        /// <param name="id">User primary key.</param>
        /// <param name="dto">Write DTO with updated values.</param>
        [HttpPut("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> Update(Guid id, [FromBody] UserCreateUpdateDTO dto)
        {
            var updated = await _service.UpdateAsync(id, dto).ConfigureAwait(false);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id).ConfigureAwait(false);
            return NoContent();
        }
    }
}
