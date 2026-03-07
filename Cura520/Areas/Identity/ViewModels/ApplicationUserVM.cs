namespace Cura520.Areas.Identity.ViewModels
{
    public class ApplicationUserVM
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
