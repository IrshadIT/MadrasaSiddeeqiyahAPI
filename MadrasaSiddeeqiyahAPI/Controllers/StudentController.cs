using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MadrasaSiddeeqiyahAPI.Data;
using MadrasaSiddeeqiyahAPI.Models;

namespace MadrasaSiddeeqiyahAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {   
            _context = context;
        }

        // POST: api/Student - Save new admission
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            student.ApplicationDate = DateTime.UtcNow;
            student.Status = "Pending";

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }

        // GET: api/Student - Get all students (for admin panel)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students.ToListAsync();
        }

        // GET: api/Student/5 - Get single student
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            return student;
        }
    }
}