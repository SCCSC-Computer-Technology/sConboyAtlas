using Microsoft.AspNetCore.Mvc;
using atlasTaskflow.Models;
using atlasTaskflow.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace atlasTaskflow.Controllers
{
    [Authorize(Policy = "InstructorOnly")]
    public class InstructorController : Controller
    {

        // db context reference
        private readonly ApplicationDbContext _context;

        public InstructorController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: InstructorController
        public async Task<IActionResult> Index()
        {
            // load the instructor's courses for the page
            var courses = await _context.Courses
                .AsNoTracking()
                .OrderBy(course => course.Name)
                .ToListAsync();

            return View(courses);
        }

        // create section ______________________________________________________________
        // create instructor page

        public IActionResult Create()
        {
            // show the course form
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseModel course)
        {
            // return the form if the course data is not valid
            if (!ModelState.IsValid)
            {
                return View(course);
            }

            // save the new course
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}
