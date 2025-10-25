using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.ViewModel
{
    public class ProfileDeleteViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }
    }
}