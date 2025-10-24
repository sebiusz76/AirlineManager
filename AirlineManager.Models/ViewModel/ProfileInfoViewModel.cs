using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.ViewModel
{
    public class ProfileInfoViewModel
    {
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
    }
}