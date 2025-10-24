using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.ViewModel
{
    public class ProfileEmailViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }
    }
}