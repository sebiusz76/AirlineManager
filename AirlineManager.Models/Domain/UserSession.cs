using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.Domain
{
    public class UserSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(256)]
        public string UserEmail { get; set; }

        /// <summary>
        /// Unique session identifier (from cookie or generated)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string SessionId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime LastActivityAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(100)]
        public string? Browser { get; set; }

        [MaxLength(100)]
        public string? OperatingSystem { get; set; }

        [MaxLength(100)]
        public string? Device { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// True if user chose "Remember Me" during login
        /// </summary>
        public bool IsPersistent { get; set; }
    }
}