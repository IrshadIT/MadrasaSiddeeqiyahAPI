using MadrasaSiddeeqiyahAPI.Data;
using MadrasaSiddeeqiyahAPI.DTOs;
using MadrasaSiddeeqiyahAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace MadrasaSiddeeqiyahAPI.Services
{
    public interface IStudentService
    {
        Task<PagedResult<StudentReadDto>> GetStudentsAsync(StudentQueryParameters queryParams);
        Task<StudentSummaryDto> GetSummaryAsync();
        Task<StudentReadDto?> GetStudentByIdAsync(int id);
        Task<StudentReadDto> CreateStudentAsync(StudentCreateDto studentDto);
        Task<bool> UpdateStudentAsync(int id, StudentUpdateDto studentDto);
        Task<bool> UpdateStudentStatusAsync(int id, string status);
        Task<bool> DeleteStudentAsync(int id);
        Task<string> ExportStudentsCsvAsync(StudentQueryParameters queryParams);
        Task<int> BulkUpdateStatusAsync(List<int> studentIds, string status);
        List<CourseDto> GetCourses();
        AadhaarVerifyResponseDto VerifyAadhaar(AadhaarVerifyRequestDto request);
    }

    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<StudentReadDto>> GetStudentsAsync(StudentQueryParameters queryParams)
        {
            var query = _context.Students.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchValue = queryParams.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(s => s.FullName.ToLower().Contains(searchValue)
                                          || s.FatherName.ToLower().Contains(searchValue)
                                          || s.Phone.Contains(searchValue));
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Status) && queryParams.Status != "All")
            {
                query = query.Where(s => s.Status == queryParams.Status);
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Course))
            {
                query = query.Where(s => s.Course == queryParams.Course);
            }

            var totalCount = await query.CountAsync();

            var pageSize = queryParams.PageSize <= 0 ? 10 : queryParams.PageSize;
            var pageNumber = queryParams.PageNumber <= 0 ? 1 : queryParams.PageNumber;

            var items = await query
                .OrderByDescending(s => s.ApplicationDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<StudentReadDto>
            {
                Items = items.Select(MapToReadDto).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<StudentSummaryDto> GetSummaryAsync()
        {
            var total = await _context.Students.CountAsync();
            var pending = await _context.Students.CountAsync(s => s.Status == "Pending");
            var approved = await _context.Students.CountAsync(s => s.Status == "Approved");
            var rejected = await _context.Students.CountAsync(s => s.Status == "Rejected");

            return new StudentSummaryDto
            {
                TotalApplications = total,
                Pending = pending,
                Approved = approved,
                Rejected = rejected
            };
        }

        public async Task<StudentReadDto?> GetStudentByIdAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            return student == null ? null : MapToReadDto(student);
        }

        public async Task<StudentReadDto> CreateStudentAsync(StudentCreateDto studentDto)
        {
            var student = new Student
            {
                FullName = studentDto.FullName,
                FatherName = studentDto.FatherName,
                Phone = studentDto.Phone,
                Course = studentDto.Course,
                HostelRequired = studentDto.HostelRequired,
                AadhaarNumber = AadhaarValidator.Mask(studentDto.AadhaarNumber),
                IsAadhaarVerified = studentDto.IsAadhaarVerified,
                AadhaarVerifiedAt = studentDto.IsAadhaarVerified ? (studentDto.AadhaarVerifiedAt ?? DateTime.UtcNow) : null,
                ApplicationDate = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return MapToReadDto(student);
        }

        public async Task<bool> UpdateStudentAsync(int id, StudentUpdateDto studentDto)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return false;
            }

            student.FullName = studentDto.FullName;
            student.FatherName = studentDto.FatherName;
            student.Phone = studentDto.Phone;
            student.Course = studentDto.Course;
            student.HostelRequired = studentDto.HostelRequired;
            if (!string.IsNullOrWhiteSpace(studentDto.AadhaarNumber))
            {
                student.AadhaarNumber = AadhaarValidator.Mask(studentDto.AadhaarNumber);
            }
            student.IsAadhaarVerified = studentDto.IsAadhaarVerified;
            student.AadhaarVerifiedAt = studentDto.AadhaarVerifiedAt;
            student.Status = studentDto.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStudentStatusAsync(int id, string status)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return false;

            student.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return false;
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> ExportStudentsCsvAsync(StudentQueryParameters queryParams)
        {
            var query = _context.Students.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchValue = queryParams.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(s => s.FullName.ToLower().Contains(searchValue)
                                          || s.FatherName.ToLower().Contains(searchValue)
                                          || s.Phone.Contains(searchValue));
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Status) && queryParams.Status != "All")
            {
                query = query.Where(s => s.Status == queryParams.Status);
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Course))
            {
                query = query.Where(s => s.Course == queryParams.Course);
            }

            var students = await query.OrderByDescending(s => s.ApplicationDate).ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Id,FullName,FatherName,Phone,Course,HostelRequired,ApplicationDate,Status");
            foreach (var student in students)
            {
                var sanitizedName = EscapeCsv(student.FullName);
                var sanitizedFather = EscapeCsv(student.FatherName);
                builder.AppendLine($"{student.Id},{sanitizedName},{sanitizedFather},{student.Phone},{EscapeCsv(student.Course)},{student.HostelRequired},{student.ApplicationDate:O},{student.Status}");
            }

            return builder.ToString();
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return '"' + value.Replace("\"", "\"\"") + '"';
            }

            return value;
        }

        public async Task<int> BulkUpdateStatusAsync(List<int> studentIds, string status)
        {
            if (studentIds == null || !studentIds.Any())
            {
                return 0;
            }

            var students = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();

            foreach (var student in students)
            {
                student.Status = status;
            }

            await _context.SaveChangesAsync();
            return students.Count;
        }

        public List<CourseDto> GetCourses()
        {
            return new List<CourseDto>
            {
                new CourseDto
                {
                    Code = "Hifz",
                    Name = "Hifz-ul-Quran",
                    Duration = "3 - 4 Years",
                    Description = "Complete memorization of the Holy Quran with Tajweed, revision routines, and individual mentorship.",
                    HostelAvailable = true
                },
                new CourseDto
                {
                    Code = "Nazira",
                    Name = "Nazira-ul-Quran",
                    Duration = "1 Year",
                    Description = "Correct recitation of the Quran with rules of Tajweed and daily basic Islamic supplications.",
                    HostelAvailable = false
                },
                new CourseDto
                {
                    Code = "Aalim",
                    Name = "Aalim Course",
                    Duration = "6 - 7 Years",
                    Description = "Comprehensive Islamic theological studies including Arabic language, Hadith, Tafseer, and Fiqh.",
                    HostelAvailable = true
                },
                new CourseDto
                {
                    Code = "Basic",
                    Name = "Basic Deeniyat",
                    Duration = "6 Months",
                    Description = "Foundational Islamic knowledge, essential daily Duas, Salah guidelines, and moral ethics.",
                    HostelAvailable = false
                }
            };
        }

        public AadhaarVerifyResponseDto VerifyAadhaar(AadhaarVerifyRequestDto request)
        {
            var result = AadhaarValidator.Validate(request?.AadhaarNumber);
            return new AadhaarVerifyResponseDto
            {
                IsValid = result.IsValid,
                IsVerified = result.IsValid,
                MaskedAadhaar = result.Masked,
                Message = result.Message,
                VerifiedAt = DateTime.UtcNow
            };
        }

        private static StudentReadDto MapToReadDto(Student student)
        {
            return new StudentReadDto
            {
                Id = student.Id,
                FullName = student.FullName,
                FatherName = student.FatherName,
                Phone = student.Phone,
                Course = student.Course,
                HostelRequired = student.HostelRequired,
                AadhaarNumber = student.AadhaarNumber,
                IsAadhaarVerified = student.IsAadhaarVerified,
                AadhaarVerifiedAt = student.AadhaarVerifiedAt,
                ApplicationDate = student.ApplicationDate,
                Status = student.Status
            };
        }
    }
}
