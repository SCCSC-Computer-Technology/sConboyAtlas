using Microsoft.AspNetCore.Mvc;

namespace ConboyAtlasTaskflow.Controllers
{
    public class InstructorController : Controller
    {
        // GET: InstructorController
        public ActionResult Index()
        {
            return View();
        }

        // create section ______________________________________________________________
        // create instructor page

        public IActionResult Create()
        {
            return View();
        }

    }
}
