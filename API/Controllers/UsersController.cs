using Common.Interfaces;
using Common.Services.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _service;

        public UsersController(IUsersService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public IActionResult GetByPK(Guid id)
        {
            var dto = _service.GetByPK(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet]
        public IActionResult GetByPage([FromQuery] int currentPage = 1, [FromQuery] int pageSize = 20)
        {
            var totalRows = 0;
            var results = _service.GetByPage(Guid.Empty, currentPage, pageSize, null, null, null, false, false, out totalRows);
            return Ok(new { TotalRows = totalRows, Items = results });
        }

        [HttpPost]
        public IActionResult Create([FromBody] UserDTO dto)
        {
            var id = _service.Create(dto);
            return CreatedAtAction(nameof(GetByPK), new { id }, null);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] UserDTO dto)
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
