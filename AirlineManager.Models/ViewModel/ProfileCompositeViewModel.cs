namespace AirlineManager.Models.ViewModel
{
    public class ProfileCompositeViewModel
    {
        public ProfileInfoViewModel Info { get; set; } = new ProfileInfoViewModel();
        public ProfileEmailViewModel Email { get; set; } = new ProfileEmailViewModel();
        public ProfilePasswordViewModel Password { get; set; } = new ProfilePasswordViewModel();
        public ProfileDeleteViewModel Delete { get; set; } = new ProfileDeleteViewModel();

        // dynamic container for two-factor details (set by controller)
        public dynamic? TwoFactor { get; set; }
    }
}