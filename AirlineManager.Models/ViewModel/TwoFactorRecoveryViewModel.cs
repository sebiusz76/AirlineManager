using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.ViewModel
{
    public class TwoFactorRecoveryViewModel
    {
        [Required]
        [Display(Name = "Recovery code")]
        public string RecoveryCode { get; set; }

        public string? ReturnUrl { get; set; }
        public string? UserId { get; set; }
    }
}