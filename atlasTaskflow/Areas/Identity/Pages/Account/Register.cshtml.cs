using System.ComponentModel.DataAnnotations;
using atlasTaskflow.Data;
using atlasTaskflow.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace atlasTaskflow.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();
        public List<SelectListItem> Organizations { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required]
            [Display(Name = "First name")]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Last name")]
            public string LastName { get; set; } = string.Empty;

            public string? Subject { get; set; }

            [Required]
            public string UserType { get; set; } = string.Empty;

            [Required]
            public int? OrganizationId { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            // load the form options when the page opens
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            await LoadOrganizationsAsync();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            // load the form options in case the page has to be shown again
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (Input.UserType != "Student" && Input.UserType != "Instructor")
            {
                ModelState.AddModelError("Input.UserType", "Choose Student or Instructor.");
            }

            var organizationExists = Input.OrganizationId.HasValue && await _context.Organizations.AnyAsync(x => x.Id == Input.OrganizationId.Value);

            if (!organizationExists)
            {
                ModelState.AddModelError("Input.OrganizationId", "Choose an existing organization.");
            }

            if (!ModelState.IsValid)
            {
                // reload organizations before returning validation errors
                await LoadOrganizationsAsync();
                return Page();
            }

            // create the identity user from the form values
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                UserType = Input.UserType,
                OrganizationId = Input.OrganizationId
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // instructors also need a profile record for instructor pages
                if (Input.UserType == "Instructor")
                {
                    _context.Instructors.Add(new InstructorModel
                    {
                        UserId = user.Id,
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        Email = Input.Email,
                        Subject = Input.Subject,
                        OrganizationId = Input.OrganizationId
                    });

                    await _context.SaveChangesAsync();
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl ?? GetDashboardUrl(user.UserType));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadOrganizationsAsync();
            return Page();
        }

        private string GetDashboardUrl(string? userType)
        {
            return userType switch
            {
                "Instructor" => Url.Action("Index", "Instructor")!,
                "Student" => Url.Action("Index", "Student")!,
                _ => Url.Content("~/")!
            };
        }

        private async Task LoadOrganizationsAsync()
        {
            // keep the organization list sorted for the dropdown
            Organizations = await _context.Organizations
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToListAsync();
        }
    }
}
