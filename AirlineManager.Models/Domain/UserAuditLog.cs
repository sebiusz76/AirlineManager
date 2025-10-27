namespace AirlineManager.Models.Domain
{
    public class UserAuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } // ID of the user affected by the change
        public string UserEmail { get; set; } // User email for easier reading
        public string ModifiedBy { get; set; } // ID of the user making the change
        public string ModifiedByEmail { get; set; } // Email of the user making the change
        public DateTime ModifiedAt { get; set; }
        public string Action { get; set; } // np. "Updated", "Deleted", "Created", "PasswordReset", "2FADisabled"
        public string? Changes { get; set; } // JSON with information about changes
        public string? OldValues { get; set; } // JSON with old values
        public string? NewValues { get; set; } // JSON with new values
    }
}