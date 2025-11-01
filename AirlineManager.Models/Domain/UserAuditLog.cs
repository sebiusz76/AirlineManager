using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlineManager.Models.Domain
{
    public class UserAuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } // ID of the user affected by the change

        [Required]
        [MaxLength(256)]
        public string UserEmail { get; set; } // User email for easier reading

        [Required]
        [MaxLength(450)]
        public string ModifiedBy { get; set; } // ID of the user making the change

        [Required]
        [MaxLength(256)]
        public string ModifiedByEmail { get; set; } // Email of the user making the change

        [Required]
        public DateTime ModifiedAt { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } // np. "Updated", "Deleted", "Created", "PasswordReset", "2FADisabled"

        public string? Changes { get; set; } // JSON with information about changes
        public string? OldValues { get; set; } // JSON with old values
        public string? NewValues { get; set; } // JSON with new values

        // Navigation Properties
        // Many-to-One: Many audit logs belong to one user (as the subject)
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        // Many-to-One: Many audit logs belong to one user (as the modifier)
        [ForeignKey(nameof(ModifiedBy))]
        public virtual ApplicationUser? Modifier { get; set; }
    }
}