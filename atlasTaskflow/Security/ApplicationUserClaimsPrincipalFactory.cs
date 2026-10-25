using System.Security.Claims;
using atlasTaskflow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace atlasTaskflow.Security
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (!string.IsNullOrWhiteSpace(user.UserType))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, user.UserType));
            }

            return identity;
        }
    }
}
