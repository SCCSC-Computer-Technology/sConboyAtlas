using atlasTaskflow.Data;
using atlasTaskflow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace atlasTaskflow.Controllers
{
    [Authorize(Policy = "InstructorOnly")]
    public class InstructorClassController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorClassController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // only show classrooms owned by the logged in instructor
            var instructor = await GetInstructorAsync();

            if (instructor == null)
            {
                return View("MissingInstructor");
            }

            var classrooms = await _context.Classrooms
                .AsNoTracking()
                .Where(x => x.InstructorId == instructor.Id)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(classrooms);
        }

        public async Task<IActionResult> Create()
        {
            // show the classroom form for the logged in instructor
            var instructor = await GetInstructorAsync();

            if (instructor == null)
            {
                return View("MissingInstructor");
            }

            return View(new ClassroomModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassroomModel classroom)
        {
            var instructor = await GetInstructorAsync();

            if (instructor == null || instructor.OrganizationId == null)
            {
                return View("MissingInstructor");
            }

            if (!ModelState.IsValid)
            {
                return View(classroom);
            }

            classroom.InstructorId = instructor.Id;
            classroom.OrganizationId = instructor.OrganizationId.Value;
            // save the classroom under the instructor's organization
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            // load the classroom and its students for the details page
            var instructor = await GetInstructorAsync();

            if (instructor == null)
            {
                return View("MissingInstructor");
            }

            var classroom = await _context.Classrooms
                .Include(x => x.Assignments)
                .FirstOrDefaultAsync(x => x.Id == id && x.InstructorId == instructor.Id);

            if (classroom == null)
            {
                return NotFound();
            }

            var enrollments = await _context.Enrollments
                .Where(x => x.ClassroomId == id)
                .ToListAsync();
            var studentIds = enrollments.Select(x => x.StudentId).ToList();
            // load the enrolled students and the students still available
            var students = await _userManager.Users
                .Where(x => studentIds.Contains(x.Id))
                .OrderBy(x => x.Email)
                .ToListAsync();

            ViewBag.Students = students;
            // only show students from the same organization who are not enrolled
            ViewBag.AvailableStudents = await _userManager.Users
                .Where(x => x.OrganizationId == classroom.OrganizationId && x.UserType == "Student" && !studentIds.Contains(x.Id))
                .OrderBy(x => x.Email)
                .Select(x => new SelectListItem { Value = x.Id, Text = x.Email! })
                .ToListAsync();

            return View(classroom);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(int classroomId, string studentId)
        {
            // make sure the instructor owns the classroom before adding anyone
            var instructor = await GetInstructorAsync();
            if (instructor == null)
            {
                return View("MissingInstructor");
            }

            var classroom = await _context.Classrooms.FirstOrDefaultAsync(x => x.Id == classroomId && x.InstructorId == instructor.Id);

            if (classroom == null)
            {
                return NotFound();
            }

            var student = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == studentId && x.UserType == "Student" && x.OrganizationId == classroom.OrganizationId);

            if (student == null)
            {
                return NotFound();
            }

            var alreadyEnrolled = await _context.Enrollments.AnyAsync(x => x.ClassroomId == classroomId && x.StudentId == studentId);

            if (!alreadyEnrolled)
            {
                // give the new student the assignments already in the classroom
                _context.Enrollments.Add(new EnrollmentModel { ClassroomId = classroomId, StudentId = studentId });
                var assignments = await _context.Assignments.Where(x => x.ClassroomId == classroomId).ToListAsync();

                foreach (var assignment in assignments)
                {
                    _context.StudentAssignments.Add(new StudentAssignmentModel { AssignmentId = assignment.Id, StudentId = studentId });
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = classroomId });
        }

        public async Task<IActionResult> CreateAssignment(int id)
        {
            var instructor = await GetInstructorAsync();

            if (instructor == null)
            {
                return View("MissingInstructor");
            }

            var classroom = await _context.Classrooms.FirstOrDefaultAsync(x => x.Id == id && x.InstructorId == instructor.Id);

            if (classroom == null)
            {
                return NotFound();
            }

            ViewBag.Classroom = classroom;
            return View(new AssignmentModel { ClassroomId = id, DueDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(AssignmentModel assignment)
        {
            // make sure the assignment belongs to one of this instructor's classrooms
            var instructor = await GetInstructorAsync();

            if (instructor == null)
            {
                return View("MissingInstructor");
            }

            var classroom = await _context.Classrooms.FirstOrDefaultAsync(x => x.Id == assignment.ClassroomId && x.InstructorId == instructor.Id);

            if (classroom == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Classroom = classroom;
                return View(assignment);
            }

            assignment.Id = 0;
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            var studentIds = await _context.Enrollments
                .Where(x => x.ClassroomId == classroom.Id)
                .Select(x => x.StudentId)
                .ToListAsync();

            foreach (var studentId in studentIds)
            {
                // create a completion record for each enrolled student
                _context.StudentAssignments.Add(new StudentAssignmentModel
                {
                    AssignmentId = assignment.Id,
                    StudentId = studentId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = classroom.Id });
        }

        private async Task<InstructorModel?> GetInstructorAsync()
        {
            // connect the logged in user to their instructor profile
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return null;
            }

            return await _context.Instructors.FirstOrDefaultAsync(x => x.UserId == user.Id);
        }
    }
}
