using ConboyAtlasTaskflow.Data;
using Microsoft.AspNetCore.Mvc;

namespace ConboyAtlasTaskflow.Controllers
{
    public class InstructorController : Controller
    {

        // db context reference
        private readonly ApplicationDbContext _context;

        public InstructorController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: InstructorController
        public ActionResult Index()
        {
            var courses = _context.Courses.ToList();

            return View(courses);
        }

        // create section ______________________________________________________________
        // create instructor page

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CourseModel course)
        {

            _context.Courses.Add(course);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
