using atlasTaskflow.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace atlasTaskflow.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // show the home page
            return View();
        }

        public IActionResult Privacy()
        {
            // show the privacy page
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // send the error details to the error page
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
