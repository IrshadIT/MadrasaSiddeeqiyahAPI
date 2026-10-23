
using MadrasaSiddeeqiyahAPI.DTOs;
using MadrasaSiddeeqiyahAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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

        // ----------------------------------------------------
        // PUBLIC FINANCE SUMMARY
        // ----------------------------------------------------

        [HttpGet("summary")]
        public async Task<ActionResult<FinanceSummaryDto>> GetSummary()
        {
            var summary =
                await _financeService.GetPublicSummaryAsync();

            return Ok(summary);
        }

        // ----------------------------------------------------
        // PUBLIC EXPENSE LIST
        // ----------------------------------------------------

        [HttpGet("expenses")]
        public async Task<ActionResult<IEnumerable<ExpenseReadDto>>> GetExpenses(
            [FromQuery] string? category,
            [FromQuery] string? campus)
        {
            var expenses =
                await _financeService.GetExpensesAsync(category, campus);

            return Ok(expenses);
        }

        // ----------------------------------------------------
        // PUBLIC DONATION LIST
        // ----------------------------------------------------

        [HttpGet("donations")]
        public async Task<ActionResult<IEnumerable<DonationReadDto>>> GetDonations()
        {
            var donations =
                await _financeService.GetDonationsAsync();

            return Ok(donations);
        }

        // ----------------------------------------------------
        // CREATE EXPENSE - ADMIN ONLY
        // ----------------------------------------------------

        [HttpPost("expenses")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExpenseReadDto>> PostExpense(
            [FromBody] ExpenseCreateDto dto)
        {
            if (dto == null
                || string.IsNullOrWhiteSpace(dto.Title)
                || dto.Amount <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "A valid expense title and positive amount are required."
                });
            }

            try
            {
                var result =
                    await _financeService.CreateExpenseAsync(dto);

                return CreatedAtAction(
                    nameof(GetExpenses),
                    new { id = result.Id },
                    result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // ----------------------------------------------------
        // CREATE DONATION - ADMIN ONLY
        // ----------------------------------------------------

        [HttpPost("donations")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DonationReadDto>> PostDonation(
            [FromBody] DonationCreateDto dto)
        {
            if (dto == null || dto.Amount <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "A valid positive donation amount is required."
                });
            }

            try
            {
                var result =
                    await _financeService.CreateDonationAsync(dto);

                return CreatedAtAction(
                    nameof(GetDonations),
                    new { id = result.Id },
                    result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
