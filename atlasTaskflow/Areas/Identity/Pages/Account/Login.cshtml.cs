using System.ComponentModel.DataAnnotations;
using atlasTaskflow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace atlasTaskflow.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            // save the page to return to after login
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            // keep the return url through the login attempt
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                // show the page again if the form has errors
                return Page();
            }

            // check the login details with identity
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // send the user to the dashboard for their role
                var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
                return LocalRedirect(returnUrl ?? GetDashboardUrl(user?.UserType));
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "This account is locked out.");
                return Page();
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "This account is not allowed to sign in yet.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        private string GetDashboardUrl(string? userType)
        {
            return userType switch
            {
                "Admin" => Url.Action("Index", "Admin")!,
                "Instructor" => Url.Action("Index", "Instructor")!,
                "Student" => Url.Action("Index", "Student")!,
                _ => Url.Content("~/")!
            };
        }
    }
}
