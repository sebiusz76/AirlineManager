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

        // Avatar URL (can store relative path or full URL)
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        // If true, user must change password at next login
        public bool MustChangePassword { get; set; } = false;

        // Date when password was last changed
        public DateTime? PasswordChangedAt { get; set; }

        // User's preferred theme: "auto", "light", or "dark"
        // "auto" = follow system preference, "light" = force light, "dark" = force dark
        [MaxLength(20)]
        public string PreferredTheme { get; set; } = "auto";

        // Navigation Properties
        // One-to-Many: User has many login history entries
        public virtual ICollection<UserLoginHistory> LoginHistories { get; set; } = new List<UserLoginHistory>();

        // One-to-Many: User has many active sessions
        public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    }
}