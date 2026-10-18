using System;
using System.Collections.Generic;

namespace MadrasaSiddeeqiyahAPI.DTOs
{
    public class ExpenseReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string SpentBy { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CampusName { get; set; } = string.Empty;
    }

    public class ExpenseCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string SpentBy { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CampusName { get; set; } = "Main Campus";
    }

    public class DonationReadDto
    {
        public int Id { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;
        public string CampusName { get; set; } = string.Empty;
    }

    public class DonationCreateDto
    {
        public string DonorName { get; set; } = "Anonymous";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Purpose { get; set; } = "General Fund";
        public string ReceiptNumber { get; set; } = string.Empty;
        public string CampusName { get; set; } = "Main Campus";
    }

    public class CategorySpendingDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }

    public class FinanceSummaryDto
    {
        public decimal TotalDonationsReceived { get; set; }
        public decimal TotalExpensesSpent { get; set; }
        public decimal NetBalance { get; set; }
        public decimal AnnualBudgetGoal { get; set; }
        public decimal RemainingBudgetNeeded { get; set; }
        public List<CategorySpendingDto> CategoryBreakdown { get; set; } = new();
        public List<ExpenseReadDto> RecentExpenses { get; set; } = new();
        public List<DonationReadDto> RecentDonations { get; set; } = new();
        public int TotalCampuses { get; set; }
        public int TotalStudentsEnrolled { get; set; }
        public int TotalCapacity { get; set; }
    }

    public class CampusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string GoogleMapEmbedUrl { get; set; } = string.Empty;
        public int StudentCapacity { get; set; }
        public int CurrentStudents { get; set; }
        public decimal AnnualBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalReceived { get; set; }
        public string InchargePerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public double CapacityUtilizationPercentage => StudentCapacity > 0 ? Math.Round((double)CurrentStudents / StudentCapacity * 100, 1) : 0;
        public decimal RemainingBudget => AnnualBudget - TotalSpent;
    }
}
