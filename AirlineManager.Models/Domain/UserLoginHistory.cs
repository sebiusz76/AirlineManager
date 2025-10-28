using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.Domain
{
    public class UserLoginHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(256)]
        public string UserEmail { get; set; }

        [Required]
        public DateTime LoginTime { get; set; }

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

        public bool IsSuccessful { get; set; }

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        public bool RequiredTwoFactor { get; set; }
    }
}