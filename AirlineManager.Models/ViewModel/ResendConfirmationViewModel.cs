using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.ViewModel
{
    public class ResendConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}