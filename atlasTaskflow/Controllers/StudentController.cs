using System.Security.Claims;
using atlasTaskflow.Data;
using atlasTaskflow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace atlasTaskflow.Controllers
{
    [Authorize(Policy = "StudentOnly")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // collect the student's dashboard information
            var studentId = GetStudentId();
            var tasks = await GetTasks(studentId);
            var assignments = await GetAssignments(studentId);
            var classes = await GetClasses(studentId);

            ViewData["TaskCount"] = await _context.Tasks.CountAsync(x => x.StudentId == studentId);
            ViewBag.Tasks = tasks;
            ViewBag.Assignments = assignments;
            ViewBag.CalendarDueDates = await GetCalendarDates(studentId);

            return View(classes);
        }

        public async Task<IActionResult> Class(int id)
        {
            // only allow the student to open a class they are enrolled in
            var studentId = GetStudentId();
            var enrollment = await _context.Enrollments
                .Include(x => x.Classroom!)
                .ThenInclude(x => x.Instructor)
                .FirstOrDefaultAsync(x => x.ClassroomId == id && x.StudentId == studentId);

            if (enrollment?.Classroom == null)
            {
                return NotFound();
            }

            var assignments = await _context.StudentAssignments
                .Include(x => x.Assignment)
                .Where(x => x.StudentId == studentId && x.Assignment!.ClassroomId == id)
                .OrderBy(x => x.IsComplete)
                .ThenBy(x => x.Assignment!.DueDate)
                .ToListAsync();

            ViewBag.Classroom = enrollment.Classroom;
            return View(assignments);
        }

        private string GetStudentId()
        {
            // get the id for whoever is logged in
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        private async Task<List<TaskModel>> GetTasks(string studentId)
        {
            // get the student's five closest tasks
            return await _context.Tasks
                .Where(x => x.StudentId == studentId)
                .OrderBy(x => x.IsComplete)
                .ThenBy(x => x.DueDate)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<StudentAssignmentModel>> GetAssignments(string studentId)
        {
            // get the student's five closest assignments
            return await _context.StudentAssignments
                .Include(x => x.Assignment)
                .ThenInclude(x => x!.Classroom)
                .Where(x => x.StudentId == studentId)
                .OrderBy(x => x.IsComplete)
                .ThenBy(x => x.Assignment!.DueDate)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<StudentClassViewModel>> GetClasses(string studentId)
        {
            // get the classes that belong to the student
            return await _context.Enrollments
                .Where(x => x.StudentId == studentId)
                .Select(x => new StudentClassViewModel
                {
                    Id = x.ClassroomId,
                    Name = x.Classroom!.Name,
                    Code = x.Classroom.Code,
                    InstructorName = x.Classroom.Instructor!.FirstName + " " + x.Classroom.Instructor.LastName,
                    AssignmentCount = x.Classroom.Assignments.Count()
                })
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        private async Task<HashSet<DateTime>> GetCalendarDates(string studentId)
        {
            // combine task and assignment dates for the calendar
            var taskDates = await _context.Tasks
                .Where(x => x.StudentId == studentId)
                .Select(x => x.DueDate)
                .ToListAsync();

            var assignmentDates = await _context.StudentAssignments
                .Where(x => x.StudentId == studentId)
                .Select(x => x.Assignment!.DueDate)
                .ToListAsync();

            return taskDates
                .Concat(assignmentDates)
                .Select(x => x.Date)
                .ToHashSet();
        }
    }
}
