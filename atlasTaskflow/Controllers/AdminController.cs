using Microsoft.AspNetCore.Mvc;
using atlasTaskflow.Models;
using atlasTaskflow.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace atlasTaskflow.Controllers
{

    // this controller will control the admin's ability to do crud stuff for instructors.
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {

        // db context reference ________________________________________________________
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // index section _______________________________________________________________
        public async Task<IActionResult> Index()
        {
            // load instructors with their organizations for the admin page
            var instructors = await _context.Instructors
                .AsNoTracking()
                .Include(instructor => instructor.Organization)
                .OrderBy(instructor => instructor.LastName)
                .ThenBy(instructor => instructor.FirstName)
                .ToListAsync();

            return View(instructors);
        }

        // create section ______________________________________________________________
        // create instructor page
        public IActionResult Create()
        {
            // load organizations for the create form
            ViewBag.Organizations = GetOrganizationOptions();
            return View();
        }

        // create instructor post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InstructorModel instructor)
        {
            if (!ModelState.IsValid)
            {
                // show the form again if the instructor data is not valid
                ViewBag.Organizations = GetOrganizationOptions();
                return View(instructor);
            }

            // save the new instructor
            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // edit section ________________________________________________________________
        // gonna need two sections similar to the create section

        public async Task<IActionResult> Edit(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);

            // if id isn't found
            if (instructor == null)
            {
                return NotFound();
            }

            ViewBag.Organizations = GetOrganizationOptions();
            return View(instructor);
        }

        // post action to save the edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InstructorModel instructor)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Organizations = GetOrganizationOptions();
                return View(instructor);
            }

            var savedInstructor = await _context.Instructors.FindAsync(instructor.Id);

            if (savedInstructor == null)
            {
                return NotFound();
            }

            // copy the edited values onto the saved instructor
            savedInstructor.FirstName = instructor.FirstName;
            savedInstructor.LastName = instructor.LastName;
            savedInstructor.Email = instructor.Email;
            savedInstructor.Subject = instructor.Subject;
            savedInstructor.OrganizationId = instructor.OrganizationId;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private List<SelectListItem> GetOrganizationOptions()
        {
            // build the organization dropdown options
            return _context.Organizations
                .OrderBy(organization => organization.Name)
                .Select(organization => new SelectListItem
                {
                    Value = organization.Id.ToString(),
                    Text = organization.Name
                })
                .ToList();
        }

        // delete section ________________________________________________________________
        public async Task<IActionResult> Delete(int id)
        {
            // find the instructor before deleting it
            var instructor = await _context.Instructors.FindAsync(id);

            if (instructor == null)
            {
                return NotFound();
            }

            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


    }
}
