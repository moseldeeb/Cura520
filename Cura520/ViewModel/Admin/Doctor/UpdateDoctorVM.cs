using Cura520.Models;
using Cura520.Validations;
using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel.Admin.Doctor
{
    public class UpdateDoctorVM
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public string Img { get; set; } = string.Empty;
       

        [EmailAddress]
        public string? Email { get; set; }


        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string?  Specialty { get; set; }


        [Phone]
        public string? PhoneNumber { get; set; }
        public decimal ConsultationFee { get; set; }

        [AllowedExtentions(new[] { ".png", ".jpg", ".jpeg", ".gif" })]
        public IFormFile? ImageFile { get; set; }
        public List<ScheduleVM> DoctorSchedules { get; set; } = [];
    }
}