using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom properties for the ApplicationUser
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }
}