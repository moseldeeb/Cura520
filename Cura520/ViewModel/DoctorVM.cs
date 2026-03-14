using Cura520.Models;
using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel
{
    public class DoctorVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        
        public Doctor Doctor { get; set; } = new Doctor();

        public List<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();
    }
}