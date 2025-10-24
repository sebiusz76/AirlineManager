namespace AirlineManager.Models.ViewModel
{
    public class AdminUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }
}