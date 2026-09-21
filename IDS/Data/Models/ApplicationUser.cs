using Microsoft.AspNetCore.Identity;

namespace IDS.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; } = false;
        public string Department { get; set; } = "General";
    }
}
