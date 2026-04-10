using Cura520.Validations;
using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel.Admin.Doctor
{
    public class CreateDoctorVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialty { get; set; }
        [Phone]
        public string PhoneNumber { get; set; }
        public decimal ConsultationFee { get; set; }

        [AllowedExtentions([".png", ".jpg", ".jpeg", ".gif"])]
        public IFormFile ImageFile { get; set; }
        public string? Img { get; set; }
        public List<ScheduleVM> DoctorSchedules { get; set; } = [];
        
    }
}