using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.ViewModel
{
    public class ProfileThemeViewModel
    {
        [Required]
        [Display(Name = "Preferred Theme")]
        public string PreferredTheme { get; set; } = "auto";
    }
}