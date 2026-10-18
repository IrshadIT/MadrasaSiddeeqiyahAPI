using System;

namespace MadrasaSiddeeqiyahAPI.Models
{
    public class Donation
    {
        public int Id { get; set; }
        public string DonorName { get; set; } = "Anonymous";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Purpose { get; set; } = "General Fund"; // Zakat, Sadaqah, Ration Sponsor, Building
        public string ReceiptNumber { get; set; } = string.Empty;
        public string CampusName { get; set; } = "Main Campus";
    }
}
