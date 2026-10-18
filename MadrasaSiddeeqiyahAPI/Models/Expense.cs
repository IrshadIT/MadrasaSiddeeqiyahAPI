using System;

namespace MadrasaSiddeeqiyahAPI.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string SpentBy { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CampusName { get; set; } = "Main Campus";
    }
}
