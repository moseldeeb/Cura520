using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel.Identity
{
    public class RegisterVM
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 25 characters")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 25 characters")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string UserName { get; set; }
        
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
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(10, ErrorMessage = "Gender must be 10 characters or less")]
        public string Gender { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number must be 20 characters or less")]
        public string PhoneNumber { get; set; }

        [StringLength(10, ErrorMessage = "Blood type must be 10 characters or less")]
        [Display(Name = "Blood Type")]
        public string BloodType { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Allergies must be 250 characters or less")]
        public string Allergies { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Address must be 150 characters or less")]
        public string Address { get; set; } = string.Empty;
    }
}
