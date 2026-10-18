using System;
namespace MadrasaSiddeeqiyahAPI.Models
{
    public class Student
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string FatherName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Course { get; set; } = string.Empty;
        
        public bool HostelRequired { get; set; }
        
        public string AadhaarNumber { get; set; } = string.Empty;

        public bool IsAadhaarVerified { get; set; } = false;

        public DateTime? AadhaarVerifiedAt { get; set; }

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Pending";
    }
}