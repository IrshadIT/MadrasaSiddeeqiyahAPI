namespace MadrasaSiddeeqiyahAPI.DTOs
{
    public class StudentCreateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public bool HostelRequired { get; set; }
        public string AadhaarNumber { get; set; } = string.Empty;
        public bool IsAadhaarVerified { get; set; } = false;
        public DateTime? AadhaarVerifiedAt { get; set; }
    }

    public class StudentReadDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public bool HostelRequired { get; set; }
        public string AadhaarNumber { get; set; } = string.Empty;
        public bool IsAadhaarVerified { get; set; }
        public DateTime? AadhaarVerifiedAt { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class StudentUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public bool HostelRequired { get; set; }
        public string AadhaarNumber { get; set; } = string.Empty;
        public bool IsAadhaarVerified { get; set; }
        public DateTime? AadhaarVerifiedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class StudentStatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class StudentQueryParameters
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Course { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class StudentSummaryDto
    {
        public int TotalApplications { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class StudentBulkStatusUpdateDto
    {
        public List<int> StudentIds { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }

    public class CourseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool HostelAvailable { get; set; }
    }

    public class HealthCheckResponseDto
    {
        public string Status { get; set; } = "Healthy";
        public string Database { get; set; } = "Connected";
        public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
        public string Environment { get; set; } = string.Empty;
    }

    public class AadhaarVerifyRequestDto
    {
        public string AadhaarNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    public class AadhaarVerifyResponseDto
    {
        public bool IsValid { get; set; }
        public bool IsVerified { get; set; }
        public string MaskedAadhaar { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;
    }
}
