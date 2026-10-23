
using MadrasaSiddeeqiyahAPI.Data;
using MadrasaSiddeeqiyahAPI.DTOs;
using MadrasaSiddeeqiyahAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadrasaSiddeeqiyahAPI.Services
{
    public interface IFinanceService
    {
        Task<FinanceSummaryDto> GetPublicSummaryAsync();

        Task<List<ExpenseReadDto>> GetExpensesAsync(
            string? category,
            string? campus);

        Task<ExpenseReadDto> CreateExpenseAsync(
            ExpenseCreateDto dto);

        Task<List<DonationReadDto>> GetDonationsAsync();

        Task<DonationReadDto> CreateDonationAsync(
            DonationCreateDto dto);

        Task<List<CampusDto>> GetCampusesAsync();

        Task<CampusDto?> GetCampusByIdAsync(int id);
    }

    public class FinanceService : IFinanceService
    {
        private readonly AppDbContext _context;

        private const string MainCampusName =
            "Main Campus - Hifz & Aalim Academy";

        public FinanceService(AppDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------
        // PUBLIC FINANCIAL SUMMARY
        // Totals are calculated from actual transactions.
        // ----------------------------------------------------

        public async Task<FinanceSummaryDto> GetPublicSummaryAsync()
        {
            var totalDonations =
                await _context.Donations
                    .AsNoTracking()
                    .SumAsync(d => (decimal?)d.Amount) ?? 0m;

            var totalExpenses =
                await _context.Expenses
                    .AsNoTracking()
                    .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var campuses = await _context.Campuses
                .AsNoTracking()
                .ToListAsync();

            var annualGoal = campuses.Sum(c => c.AnnualBudget);

            if (annualGoal <= 0)
            {
                annualGoal = 3000000m;
            }

            var recentExpenses = await _context.Expenses
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .Take(15)
                .Select(e => MapExpense(e))
                .ToListAsync();

            var recentDonations = await _context.Donations
                .AsNoTracking()
                .OrderByDescending(d => d.Date)
                .Take(10)
                .Select(d => MapDonation(d))
                .ToListAsync();

            var expenseList = await _context.Expenses
                .AsNoTracking()
                .ToListAsync();

            var categoryBreakdown =
                new List<CategorySpendingDto>();

            if (totalExpenses > 0)
            {
                categoryBreakdown = expenseList
                    .GroupBy(e => e.Category)
                    .Select(g => new CategorySpendingDto
                    {
                        Category = g.Key,
                        Amount = g.Sum(e => e.Amount),
                        Percentage = Math.Round(
                            (double)(
                                g.Sum(e => e.Amount) / totalExpenses
                            ) * 100,
                            1)
                    })
                    .OrderByDescending(c => c.Amount)
                    .ToList();
            }

            return new FinanceSummaryDto
            {
                TotalDonationsReceived = totalDonations,
                TotalExpensesSpent = totalExpenses,
                NetBalance = totalDonations - totalExpenses,
                AnnualBudgetGoal = annualGoal,

                RemainingBudgetNeeded = Math.Max(
                    0m,
                    annualGoal - totalDonations),

                CategoryBreakdown = categoryBreakdown,
                RecentExpenses = recentExpenses,
                RecentDonations = recentDonations,
                TotalCampuses = campuses.Count,

                TotalStudentsEnrolled =
                    campuses.Sum(c => c.CurrentStudents),

                TotalCapacity =
                    campuses.Sum(c => c.StudentCapacity)
            };
        }

        // ----------------------------------------------------
        // GET EXPENSES
        // ----------------------------------------------------

        public async Task<List<ExpenseReadDto>> GetExpensesAsync(
            string? category,
            string? campus)
        {
            var query = _context.Expenses.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(category)
                && !string.Equals(
                    category,
                    "All",
                    StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => e.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(campus)
                && !string.Equals(
                    campus,
                    "All",
                    StringComparison.OrdinalIgnoreCase))
            {
                var campusFilter =
                    await ResolveCampusNameAsync(campus);

                query = query.Where(
                    e => e.CampusName == campusFilter);
            }

            return await query
                .OrderByDescending(e => e.Date)
                .Select(e => MapExpense(e))
                .ToListAsync();
        }

        // ----------------------------------------------------
        // CREATE EXPENSE
        // Validate the campus before saving the transaction.
        // Transaction records are the source of truth.
        // ----------------------------------------------------

        public async Task<ExpenseReadDto> CreateExpenseAsync(
            ExpenseCreateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException(
                    "An expense request is required.",
                    nameof(dto));
            }

            var campusName =
                await ResolveAndValidateCampusNameAsync(
                    dto.CampusName);

            var expense = new Expense
            {
                Title = dto.Title,
                Category = dto.Category,
                Amount = dto.Amount,

                Date = dto.Date == default
                    ? DateTime.UtcNow
                    : dto.Date,

                SpentBy = dto.SpentBy,
                ReceiptNumber =
                    string.IsNullOrWhiteSpace(dto.ReceiptNumber)
                        ? $"VCH-{DateTime.UtcNow.Ticks % 1000000}"
                        : dto.ReceiptNumber,

                Description = dto.Description,
                CampusName = campusName
            };

            _context.Expenses.Add(expense);

            await _context.SaveChangesAsync();

            return MapExpense(expense);
        }

        // ----------------------------------------------------
        // GET DONATIONS
        // ----------------------------------------------------

        public async Task<List<DonationReadDto>> GetDonationsAsync()
        {
            return await _context.Donations
                .AsNoTracking()
                .OrderByDescending(d => d.Date)
                .Select(d => MapDonation(d))
                .ToListAsync();
        }

        // ----------------------------------------------------
        // CREATE DONATION
        // Validate the campus before saving the transaction.
        // Transaction records are the source of truth.
        // ----------------------------------------------------

        public async Task<DonationReadDto> CreateDonationAsync(
            DonationCreateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException(
                    "A donation request is required.",
                    nameof(dto));
            }

            var campusName =
                await ResolveAndValidateCampusNameAsync(
                    dto.CampusName);

            var donation = new Donation
            {
                DonorName =
                    string.IsNullOrWhiteSpace(dto.DonorName)
                        ? "Anonymous"
                        : dto.DonorName.Trim(),

                Amount = dto.Amount,

                Date = dto.Date == default
                    ? DateTime.UtcNow
                    : dto.Date,

                Purpose = dto.Purpose,

                ReceiptNumber =
                    string.IsNullOrWhiteSpace(dto.ReceiptNumber)
                        ? $"DON-{DateTime.UtcNow.Ticks % 1000000}"
                        : dto.ReceiptNumber.Trim(),

                CampusName = campusName
            };

            _context.Donations.Add(donation);

            await _context.SaveChangesAsync();

            return MapDonation(donation);
        }

        // ----------------------------------------------------
        // GET ALL CAMPUSES
        // Calculate campus totals using transaction records.
        // ----------------------------------------------------

        public async Task<List<CampusDto>> GetCampusesAsync()
        {
            var campuses = await _context.Campuses
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();

            var expenseTotals = await _context.Expenses
                .AsNoTracking()
                .GroupBy(e => e.CampusName)
                .Select(g => new
                {
                    CampusName = g.Key,
                    Total = g.Sum(e => e.Amount)
                })
                .ToDictionaryAsync(
                    x => x.CampusName,
                    x => x.Total);

            var donationTotals = await _context.Donations
                .AsNoTracking()
                .GroupBy(d => d.CampusName)
                .Select(g => new
                {
                    CampusName = g.Key,
                    Total = g.Sum(d => d.Amount)
                })
                .ToDictionaryAsync(
                    x => x.CampusName,
                    x => x.Total);

            return campuses.Select(c =>
            {
                expenseTotals.TryGetValue(
                    c.Name,
                    out var totalSpent);

                donationTotals.TryGetValue(
                    c.Name,
                    out var totalReceived);

                return MapCampus(
                    c,
                    totalSpent,
                    totalReceived);
            }).ToList();
        }

        // ----------------------------------------------------
        // GET CAMPUS BY ID
        // Use the same transaction-based calculations.
        // ----------------------------------------------------

        public async Task<CampusDto?> GetCampusByIdAsync(int id)
        {
            var campus = await _context.Campuses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campus == null)
            {
                return null;
            }

            var totalSpent = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.CampusName == campus.Name)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var totalReceived = await _context.Donations
                .AsNoTracking()
                .Where(d => d.CampusName == campus.Name)
                .SumAsync(d => (decimal?)d.Amount) ?? 0m;

            return MapCampus(
                campus,
                totalSpent,
                totalReceived);
        }

        // ----------------------------------------------------
        // RESOLVE CAMPUS NAME
        // Accept a campus name or branch code.
        // A blank value defaults to the main campus.
        // ----------------------------------------------------

        private async Task<string> ResolveCampusNameAsync(
            string? requestedCampusName)
        {
            var campuses = await _context.Campuses
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (string.IsNullOrWhiteSpace(requestedCampusName))
            {
                var mainCampus = campuses.FirstOrDefault(c =>
                    string.Equals(
                        c.Name,
                        MainCampusName,
                        StringComparison.OrdinalIgnoreCase));

                return mainCampus?.Name
                    ?? campuses.FirstOrDefault()?.Name
                    ?? MainCampusName;
            }

            var requested = requestedCampusName.Trim();

            var matchedCampus = campuses.FirstOrDefault(c =>
                string.Equals(
                    c.Name,
                    requested,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    c.BranchCode,
                    requested,
                    StringComparison.OrdinalIgnoreCase));

            if (matchedCampus != null)
            {
                return matchedCampus.Name;
            }

            // Support the existing default value "Main Campus".
            if (string.Equals(
                requested,
                "Main Campus",
                StringComparison.OrdinalIgnoreCase))
            {
                var mainCampus = campuses.FirstOrDefault(c =>
                    string.Equals(
                        c.BranchCode,
                        "MS-HYD-01",
                        StringComparison.OrdinalIgnoreCase));

                return mainCampus?.Name
                    ?? campuses.FirstOrDefault()?.Name
                    ?? MainCampusName;
            }

            // Keep an unknown name so the validator below
            // can reject it instead of silently accepting it.
            return requested;
        }

        // ----------------------------------------------------
        // VALIDATE CAMPUS
        // Reject a campus that does not exist before saving.
        // ----------------------------------------------------

        private async Task<string> ResolveAndValidateCampusNameAsync(
            string? requestedCampusName)
        {
            var campusName =
                await ResolveCampusNameAsync(requestedCampusName);

            var exists = await _context.Campuses
                .AsNoTracking()
                .AnyAsync(c => c.Name == campusName);

            if (!exists)
            {
                var suppliedName =
                    string.IsNullOrWhiteSpace(requestedCampusName)
                        ? "(empty)"
                        : requestedCampusName.Trim();

                throw new ArgumentException(
                    $"Campus '{suppliedName}' was not found. " +
                    "Provide a valid campus name or branch code.",
                    nameof(requestedCampusName));
            }

            return campusName;
        }

        // ----------------------------------------------------
        // DTO MAPPING
        // ----------------------------------------------------

        private static ExpenseReadDto MapExpense(Expense e)
        {
            return new ExpenseReadDto
            {
                Id = e.Id,
                Title = e.Title,
                Category = e.Category,
                Amount = e.Amount,
                Date = e.Date,
                SpentBy = e.SpentBy,
                ReceiptNumber = e.ReceiptNumber,
                Description = e.Description,
                CampusName = e.CampusName
            };
        }

        private static DonationReadDto MapDonation(Donation d)
        {
            return new DonationReadDto
            {
                Id = d.Id,
                DonorName = d.DonorName,
                Amount = d.Amount,
                Date = d.Date,
                Purpose = d.Purpose,
                ReceiptNumber = d.ReceiptNumber,
                CampusName = d.CampusName
            };
        }

        private static CampusDto MapCampus(
            Campus c,
            decimal totalSpent,
            decimal totalReceived)
        {
            return new CampusDto
            {
                Id = c.Id,
                Name = c.Name,
                BranchCode = c.BranchCode,
                Address = c.Address,
                City = c.City,
                State = c.State,
                Latitude = c.Latitude,
                Longitude = c.Longitude,
                GoogleMapEmbedUrl = c.GoogleMapEmbedUrl,
                StudentCapacity = c.StudentCapacity,
                CurrentStudents = c.CurrentStudents,
                AnnualBudget = c.AnnualBudget,
                TotalSpent = totalSpent,
                TotalReceived = totalReceived,
                InchargePerson = c.InchargePerson,
                ContactPhone = c.ContactPhone
            };
        }
    }
}
