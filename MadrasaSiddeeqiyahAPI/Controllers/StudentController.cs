using Microsoft.AspNetCore.Mvc;
using MadrasaSiddeeqiyahAPI.DTOs;
using MadrasaSiddeeqiyahAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace MadrasaSiddeeqiyahAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // POST: api/Student - Save new admission (Public)
        [HttpPost]
        public async Task<ActionResult<StudentReadDto>> PostStudent(StudentCreateDto studentDto)
        {
            var result = await _studentService.CreateStudentAsync(studentDto);
            return CreatedAtAction(nameof(GetStudent), new { id = result.Id }, result);
        }

        // GET: api/Student - Get paginated student list (Admin Only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<StudentReadDto>>> GetStudents([FromQuery] StudentQueryParameters queryParams)
        {
            var pagedResult = await _studentService.GetStudentsAsync(queryParams);
            return Ok(pagedResult);
        }

        // GET: api/Student/summary - Summary stats for admin dashboard
        [HttpGet("summary")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentSummaryDto>> GetSummary()
        {
            var summary = await _studentService.GetSummaryAsync();
            return Ok(summary);
        }

        // GET: api/Student/export - Export filtered applications as CSV
        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Export([FromQuery] StudentQueryParameters queryParams)
        {
            var csv = await _studentService.ExportStudentsCsvAsync(queryParams);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "student-applications.csv");
        }

        // GET: api/Student/5 - Get single student
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentReadDto>> GetStudent(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        // PUT: api/Student/5 - Update student details (Admin Only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentUpdateDto studentDto)
        {
            var success = await _studentService.UpdateStudentAsync(id, studentDto);
            if (!success) return NotFound();
            return NoContent();
        }

        // PUT: api/Student/5/status - Update student status (Admin Only)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StudentStatusUpdateDto statusUpdate)
        {
            var success = await _studentService.UpdateStudentStatusAsync(id, statusUpdate.Status);
            if (!success) return NotFound();
            return NoContent();
        }

        // GET: api/Student/courses - Get available courses with details (Public)
        [HttpGet("courses")]
        public ActionResult<IEnumerable<CourseDto>> GetCourses()
        {
            var courses = _studentService.GetCourses();
            return Ok(courses);
        }

        // POST: api/Student/verify-aadhaar - Validate and verify an Aadhaar number using UIDAI Verhoeff algorithm (Public)
        [HttpPost("verify-aadhaar")]
        public ActionResult<AadhaarVerifyResponseDto> VerifyAadhaar([FromBody] AadhaarVerifyRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AadhaarNumber))
            {
                return BadRequest(new AadhaarVerifyResponseDto
                {
                    IsValid = false,
                    IsVerified = false,
                    Message = "Aadhaar number is required."
                });
            }

            var result = _studentService.VerifyAadhaar(request);
            if (!result.IsValid)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Student/bulk-status - Bulk update student statuses (Admin Only)
        [HttpPost("bulk-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkUpdateStatus([FromBody] StudentBulkStatusUpdateDto bulkDto)
        {
            if (bulkDto == null || !bulkDto.StudentIds.Any() || string.IsNullOrWhiteSpace(bulkDto.Status))
            {
                return BadRequest(new { message = "Student IDs and a valid status are required." });
            }

            var updatedCount = await _studentService.BulkUpdateStatusAsync(bulkDto.StudentIds, bulkDto.Status);
            return Ok(new { updatedCount, message = $"Successfully updated {updatedCount} applications to {bulkDto.Status}." });
        }

        // DELETE: api/Student/5 - Remove a student application (Admin Only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var success = await _studentService.DeleteStudentAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
