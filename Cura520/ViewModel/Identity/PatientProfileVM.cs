using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel.Identity
{
    /// <summary>
    /// Patient profile completion - collected after first login
    /// All fields optional to allow progressive profile completion
    /// </summary>
    public class PatientProfileVM
    {
        public int PatientId { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(10, ErrorMessage = "Gender must be 10 characters or less")]
        public string Gender { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number must be 20 characters or less")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(10, ErrorMessage = "Blood type must be 10 characters or less")]
        [Display(Name = "Blood Type")]
        public string BloodType { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Allergies must be 250 characters or less")]
        public string Allergies { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Address must be 150 characters or less")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Profile Photo")]
        public IFormFile? ProfilePhoto { get; set; }

        public string? ProfilePhotoUrl { get; set; }

        // Progress indicator
        [Display(Name = "Profile Complete")]
        public bool IsProfileComplete { get; set; }
    }
}
