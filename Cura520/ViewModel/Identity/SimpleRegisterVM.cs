using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel.Identity
{
    /// <summary>
    /// Simplified registration - only basic account info required
    /// Additional patient info collected after login on profile page
    /// </summary>
    public class SimpleRegisterVM
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 25 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 25 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
