using System.Security.Claims;
using atlasTaskflow.Data;
using atlasTaskflow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace atlasTaskflow.Controllers
{
    [Authorize(Policy = "StudentOnly")]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // load personal tasks and class assignments together
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tasks = await _context.Tasks
                .Where(task => task.StudentId == studentId)
                .OrderBy(task => task.IsComplete)
                .ThenBy(task => task.DueDate)
                .ToListAsync();

            var assignments = await _context.StudentAssignments
                .Include(x => x.Assignment)
                .ThenInclude(x => x!.Classroom)
                .Where(x => x.StudentId == studentId)
                .OrderBy(x => x.IsComplete)
                .ThenBy(x => x.Assignment!.DueDate)
                .ToListAsync();

            ViewBag.Assignments = assignments;
            return View(tasks);
        }

        public IActionResult Create()
        {
            // show the task form
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskModel task)
        {
            // assign the task to the logged in student
            task.StudentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            ModelState.Remove(nameof(TaskModel.StudentId));

            if (!ModelState.IsValid)
            {
                return View(task);
            }

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Student");
        }

        public async Task<IActionResult> Edit(int id)
        {
            // only find tasks owned by the logged in student
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var task = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaskModel task)
        {
            // find the saved task so another student cannot edit it
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var savedTask = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == task.Id && x.StudentId == studentId);

            if (savedTask == null)
            {
                return NotFound();
            }

            task.StudentId = studentId!;
            ModelState.Remove(nameof(TaskModel.StudentId));

            if (!ModelState.IsValid)
            {
                return View(task);
            }

            savedTask.Name = task.Name;
            savedTask.Description = task.Description;
            savedTask.DueDate = task.DueDate;
            savedTask.IsComplete = task.IsComplete;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Student");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            // flip the task status and save it
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var task = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);

            if (task == null)
            {
                return NotFound();
            }

            task.IsComplete = !task.IsComplete;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Student");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAssignmentComplete(int id)
        {
            // flip the assignment status and save it
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var assignment = await _context.StudentAssignments.FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);

            if (assignment == null)
            {
                return NotFound();
            }

            assignment.IsComplete = !assignment.IsComplete;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Student");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // only delete tasks owned by the logged in student
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var task = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);

            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Student");
        }
    }
}
