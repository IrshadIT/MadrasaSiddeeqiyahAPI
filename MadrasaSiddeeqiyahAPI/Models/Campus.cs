namespace MadrasaSiddeeqiyahAPI.Models
{
    public class Campus
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
    }
}
