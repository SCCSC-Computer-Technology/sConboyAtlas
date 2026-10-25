using atlasTaskflow.Data;
using atlasTaskflow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace atlasTaskflow.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class OrganizationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrganizationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // load organizations in a predictable order for the admin page
            var organizations = await _context.Organizations
                .AsNoTracking()
                .OrderBy(organization => organization.Name)
                .ToListAsync();

            return View(organizations);
        }

        public IActionResult Create()
        {
            // show the organization form
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganizationModel organization)
        {
            // return the form if the organization data is not valid
            if (!ModelState.IsValid)
            {
                return View(organization);
            }

            // save the new organization
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // find the organization before trying to remove it
            var organization = await _context.Organizations.FindAsync(id);

            if (organization == null)
            {
                return NotFound();
            }

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
