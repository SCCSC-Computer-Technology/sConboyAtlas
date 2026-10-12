using Microsoft.AspNetCore.Mvc;
using ConboyAtlasTaskflow.Data;


namespace ConboyAtlasTaskflow.Controllers
{

    // this controller will control the admin's ability to do crud stuff for instructors.
    public class AdminController : Controller
    {

        // db context reference
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminController
        public ActionResult Index()
        {
            var instructors = _context.Instructors.ToList();

            return View(instructors);
        }

        // create section ____________________________________________________________
        // create instructor page
        public IActionResult Create()
        {
            return View();
        }

        // create instructor post
        [HttpPost]
        public IActionResult Create(InstructorModel instructor)
        {
            _context.Instructors.Add(instructor);
            _context.SaveChanges();


            return RedirectToAction("Index");
        }

    }
}
