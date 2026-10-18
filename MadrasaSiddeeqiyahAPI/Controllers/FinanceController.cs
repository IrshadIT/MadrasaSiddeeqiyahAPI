using MadrasaSiddeeqiyahAPI.DTOs;
using MadrasaSiddeeqiyahAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MadrasaSiddeeqiyahAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        /// <summary>
        /// Public Transparency Summary: Total funds received, total spent, balance, deficit/needed, and category allocations.
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<FinanceSummaryDto>> GetSummary()
        {
            var summary = await _financeService.GetPublicSummaryAsync();
            return Ok(summary);
        }

        /// <summary>
        /// Public list of itemized expenses & purchases.
        /// </summary>
        [HttpGet("expenses")]
        public async Task<ActionResult<IEnumerable<ExpenseReadDto>>> GetExpenses([FromQuery] string? category, [FromQuery] string? campus)
        {
            var expenses = await _financeService.GetExpensesAsync(category, campus);
            return Ok(expenses);
        }

        /// <summary>
        /// Public list of recorded donations received.
        /// </summary>
        [HttpGet("donations")]
        public async Task<ActionResult<IEnumerable<DonationReadDto>>> GetDonations()
        {
            var donations = await _financeService.GetDonationsAsync();
            return Ok(donations);
        }

        /// <summary>
        /// Add a new expense (Admin Only).
        /// </summary>
        [HttpPost("expenses")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExpenseReadDto>> PostExpense([FromBody] ExpenseCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title) || dto.Amount <= 0)
            {
                return BadRequest(new { message = "Valid expense title and positive amount are required." });
            }

            var result = await _financeService.CreateExpenseAsync(dto);
            return CreatedAtAction(nameof(GetExpenses), new { id = result.Id }, result);
        }

        /// <summary>
        /// Record a new donation (Admin Only).
        /// </summary>
        [HttpPost("donations")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DonationReadDto>> PostDonation([FromBody] DonationCreateDto dto)
        {
            if (dto == null || dto.Amount <= 0)
            {
                return BadRequest(new { message = "Valid positive donation amount is required." });
            }

            var result = await _financeService.CreateDonationAsync(dto);
            return CreatedAtAction(nameof(GetDonations), new { id = result.Id }, result);
        }
    }
}
