using System.ComponentModel.DataAnnotations;

namespace AirlineManager.Models.ViewModel
{
    public class AdminEditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public List<string> SelectedRoles { get; set; } = new List<string>();
        public List<string> AllRoles { get; set; } = new List<string>();
        public bool IsLockedOut { get; set; }
    }
}