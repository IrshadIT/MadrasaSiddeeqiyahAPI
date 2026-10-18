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
        Task<List<ExpenseReadDto>> GetExpensesAsync(string? category, string? campus);
        Task<ExpenseReadDto> CreateExpenseAsync(ExpenseCreateDto dto);
        Task<List<DonationReadDto>> GetDonationsAsync();
        Task<DonationReadDto> CreateDonationAsync(DonationCreateDto dto);
        Task<List<CampusDto>> GetCampusesAsync();
        Task<CampusDto?> GetCampusByIdAsync(int id);
    }

    public class FinanceService : IFinanceService
    {
        private readonly AppDbContext _context;

        public FinanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FinanceSummaryDto> GetPublicSummaryAsync()
        {
            var totalDonations = await _context.Donations.SumAsync(d => (decimal?)d.Amount) ?? 0m;
            var totalExpenses = await _context.Expenses.SumAsync(e => (decimal?)e.Amount) ?? 0m;
            var campuses = await _context.Campuses.ToListAsync();

            var annualGoal = campuses.Sum(c => c.AnnualBudget);
            if (annualGoal <= 0) annualGoal = 3000000m; // Default ₹30 Lakhs overall goal

            var recentExpenses = await _context.Expenses
                .OrderByDescending(e => e.Date)
                .Take(15)
                .Select(e => MapExpense(e))
                .ToListAsync();

            var recentDonations = await _context.Donations
                .OrderByDescending(d => d.Date)
                .Take(10)
                .Select(d => MapDonation(d))
                .ToListAsync();

            // Calculate category breakdown
            var expenseList = await _context.Expenses.ToListAsync();
            var categoryBreakdown = new List<CategorySpendingDto>();

            if (totalExpenses > 0)
            {
                categoryBreakdown = expenseList
                    .GroupBy(e => e.Category)
                    .Select(g => new CategorySpendingDto
                    {
                        Category = g.Key,
                        Amount = g.Sum(e => e.Amount),
                        Percentage = Math.Round((double)(g.Sum(e => e.Amount) / totalExpenses) * 100, 1)
                    })
                    .OrderByDescending(c => c.Amount)
                    .ToList();
            }

            var totalStudents = campuses.Sum(c => c.CurrentStudents);
            var totalCapacity = campuses.Sum(c => c.StudentCapacity);

            return new FinanceSummaryDto
            {
                TotalDonationsReceived = totalDonations,
                TotalExpensesSpent = totalExpenses,
                NetBalance = totalDonations - totalExpenses,
                AnnualBudgetGoal = annualGoal,
                RemainingBudgetNeeded = Math.Max(0, annualGoal - totalDonations),
                CategoryBreakdown = categoryBreakdown,
                RecentExpenses = recentExpenses,
                RecentDonations = recentDonations,
                TotalCampuses = campuses.Count,
                TotalStudentsEnrolled = totalStudents,
                TotalCapacity = totalCapacity
            };
        }

        public async Task<List<ExpenseReadDto>> GetExpensesAsync(string? category, string? campus)
        {
            var query = _context.Expenses.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                query = query.Where(e => e.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(campus) && campus != "All")
            {
                query = query.Where(e => e.CampusName == campus);
            }

            return await query
                .OrderByDescending(e => e.Date)
                .Select(e => MapExpense(e))
                .ToListAsync();
        }

        public async Task<ExpenseReadDto> CreateExpenseAsync(ExpenseCreateDto dto)
        {
            var expense = new Expense
            {
                Title = dto.Title,
                Category = dto.Category,
                Amount = dto.Amount,
                Date = dto.Date == default ? DateTime.UtcNow : dto.Date,
                SpentBy = dto.SpentBy,
                ReceiptNumber = string.IsNullOrWhiteSpace(dto.ReceiptNumber) ? $"VCH-{DateTime.UtcNow.Ticks % 1000000}" : dto.ReceiptNumber,
                Description = dto.Description,
                CampusName = string.IsNullOrWhiteSpace(dto.CampusName) ? "Main Campus" : dto.CampusName
            };

            _context.Expenses.Add(expense);

            // Update campus total spent if exists
            var campus = await _context.Campuses.FirstOrDefaultAsync(c => c.Name == expense.CampusName);
            if (campus != null)
            {
                campus.TotalSpent += expense.Amount;
            }

            await _context.SaveChangesAsync();
            return MapExpense(expense);
        }

        public async Task<List<DonationReadDto>> GetDonationsAsync()
        {
            return await _context.Donations
                .AsNoTracking()
                .OrderByDescending(d => d.Date)
                .Select(d => MapDonation(d))
                .ToListAsync();
        }

        public async Task<DonationReadDto> CreateDonationAsync(DonationCreateDto dto)
        {
            var donation = new Donation
            {
                DonorName = string.IsNullOrWhiteSpace(dto.DonorName) ? "Anonymous" : dto.DonorName,
                Amount = dto.Amount,
                Date = dto.Date == default ? DateTime.UtcNow : dto.Date,
                Purpose = dto.Purpose,
                ReceiptNumber = string.IsNullOrWhiteSpace(dto.ReceiptNumber) ? $"DON-{DateTime.UtcNow.Ticks % 1000000}" : dto.ReceiptNumber,
                CampusName = string.IsNullOrWhiteSpace(dto.CampusName) ? "Main Campus" : dto.CampusName
            };

            _context.Donations.Add(donation);

            var campus = await _context.Campuses.FirstOrDefaultAsync(c => c.Name == donation.CampusName);
            if (campus != null)
            {
                campus.TotalReceived += donation.Amount;
            }

            await _context.SaveChangesAsync();
            return MapDonation(donation);
        }

        public async Task<List<CampusDto>> GetCampusesAsync()
        {
            return await _context.Campuses
                .AsNoTracking()
                .Select(c => MapCampus(c))
                .ToListAsync();
        }

        public async Task<CampusDto?> GetCampusByIdAsync(int id)
        {
            var campus = await _context.Campuses.FindAsync(id);
            if (campus == null) return null;
            return MapCampus(campus);
        }

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

        private static CampusDto MapCampus(Campus c)
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
                TotalSpent = c.TotalSpent,
                TotalReceived = c.TotalReceived,
                InchargePerson = c.InchargePerson,
                ContactPhone = c.ContactPhone
            };
        }
    }
}
