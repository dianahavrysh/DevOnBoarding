using Common.DTOs;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using API.Filters;
using System.Collections.Generic;
using Common.Entities;

namespace API.Controllers {
    /// <summary>
    /// API controller exposing user-related endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase {
        private readonly IUsersService _service;
        private readonly ITokenClaimsService _tokenClaimsService;

        /// <summary>
        /// Creates a new <see cref="UsersController"/>.
        /// </summary>
        /// <param name="service">Service providing user operations.</param>
        /// <param name="tokenClaimsService">Service for extracting claims from the authenticated user.</param>
        public UsersController(IUsersService service, ITokenClaimsService tokenClaimsService) {
            _service = service;
            _tokenClaimsService = tokenClaimsService;
        }

        /// <summary>
        /// Get a user by primary key.
        /// </summary>
        /// <param name="id">User primary key.</param>
        /// <returns>200 with user DTO or 404 if not found.</returns>
        [RequireAuth]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByPK(Guid id) {
            var dto = await _service.GetByPKAsync(id);

            return dto is null
                ? NotFound()
                : Ok(dto);
        }

        /// <summary>
        /// Get a paginated list of users.
        /// </summary>
        [RequireAuth]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
            var requestingUserPK = _tokenClaimsService.GetUserIdFromPrincipal(User);

            var searchByFields = new Dictionary<string, bool>
            {
                { nameof(Common.Entities.User.UserName), searchByUserName },
                { nameof(Common.Entities.User.Email), searchByEmail },
                { nameof(Common.Entities.User.FirstName), searchByFirstName },
                { nameof(Common.Entities.User.SecondName), searchBySecondName }
            };

            var (items, totalRows) = await _service.GetByPageAsync(
                requestingUserPK,
                currentPage,
                pageSize,
                sortExpression,
                searchValue,
                searchByFields,
                includeInactive,
                strictMatch);

            return Ok(new { items, totalRows });
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="dto">Write DTO containing user data.</param>
        /// <returns>201 Created with the created user.</returns>
        [RequireAuth]
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
        [RequireAuth]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UserCreateUpdateDTO dto) {
            var updated = await _service.UpdateAsync(dto);

            return updated
                ? NoContent()
                : NotFound();
        }

        /// <summary>
        /// Delete the user identified by the specified primary key.
        /// </summary>
        /// <param name="id">User primary key.</param>
        [RequireAuth]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id) {
            await _service.DeleteAsync(id);

            return NoContent();
        }
    }
}
