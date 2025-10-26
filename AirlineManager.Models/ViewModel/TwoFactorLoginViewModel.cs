using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.ViewModel
{
    public class TwoFactorLoginViewModel
    {
        [Required]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; }

        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
        public string? UserId { get; set; }
    }
}