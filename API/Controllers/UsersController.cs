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
        public IActionResult GetByPK(Guid id)
        {
            var dto = _service.GetByPK(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Get a paginated list of users.
        /// </summary>
        [HttpGet]
        public IActionResult GetByPage(
        [FromQuery] Guid requestingUserPK,
        [FromQuery] int currentPage = 1,
        [FromQuery] int pageSize = 20) {
            var totalRows = 0;
            var results = _service.GetByPage(requestingUserPK, currentPage, pageSize, null, null, null, false, false, out totalRows);
            return Ok(new { TotalRows = totalRows, Items = results });
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="dto">Write DTO containing user data.</param>
        /// <returns>201 Created with Location header to the new resource.</returns>
        [HttpPost]
        public IActionResult Create([FromBody] UserCreateUpdateDTO dto)
        {
            var id = _service.Create(dto);
            return CreatedAtAction(nameof(GetByPK), new { id }, null);
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        /// <param name="id">User primary key.</param>
        /// <param name="dto">Write DTO with updated values.</param>
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] UserCreateUpdateDTO dto)
        {
            try
            {
                _service.Update(id, dto);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _service.Delete(id);
            return NoContent();
        }
    }
}
