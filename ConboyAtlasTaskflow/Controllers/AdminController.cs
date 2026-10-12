using Microsoft.AspNetCore.Mvc;
using ConboyAtlasTaskflow.Data;


namespace ConboyAtlasTaskflow.Controllers
{

    // this controller will control the admin's ability to do crud stuff for instructors.
    public class AdminController : Controller
    {

        // db context reference ________________________________________________________
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // index section _______________________________________________________________
        public ActionResult Index()
        {
            var instructors = _context.Instructors.ToList();

            return View(instructors);
        }

        // create section ______________________________________________________________
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

        // edit section ________________________________________________________________
        // gonna need two sections similar to the create section

        public IActionResult Edit(int id)
        {
            var instructor = _context.Instructors.Find(id);

            // if id isn't found
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // post action to save the edit
        [HttpPost]
        public IActionResult Edit(InstructorModel instructor)
        {
            _context.Instructors.Update(instructor);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // delete section ________________________________________________________________
        public IActionResult Delete(int id)
        {
            var instructor = _context.Instructors.Find(id);

            if (instructor == null)
            {
                return NotFound();
            }

            _context.Instructors.Remove(instructor);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
