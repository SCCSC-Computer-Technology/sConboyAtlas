using Microsoft.AspNetCore.Identity;

namespace atlasTaskflow.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string UserType { get; set; } = string.Empty;
        public int? OrganizationId { get; set; }
        public OrganizationModel? Organization { get; set; }
    }
}
