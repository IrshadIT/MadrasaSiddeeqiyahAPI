using MadrasaSiddeeqiyahAPI.DTOs;
using MadrasaSiddeeqiyahAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MadrasaSiddeeqiyahAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampusController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public CampusController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        /// <summary>
        /// Get all Madrasa Campuses/Branches with calculations, student capacity, budget vs spent, and Google Map details.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CampusDto>>> GetCampuses()
        {
            var campuses = await _financeService.GetCampusesAsync();
            return Ok(campuses);
        }

        /// <summary>
        /// Get specific campus by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CampusDto>> GetCampus(int id)
        {
            var campus = await _financeService.GetCampusByIdAsync(id);
            if (campus == null) return NotFound();
            return Ok(campus);
        }
    }
}
