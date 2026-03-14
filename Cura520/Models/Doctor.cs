using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cura520.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Img { get; set; } = "defaultImg.png";
        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }
        [Required]
        public string Specialty { get; set; }
        [Phone]
        public string PhoneNumber { get; set; }
        public decimal ConsultationFee { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Prescription>? Prescriptions { get; set; }
        public ICollection<DoctorSchedule>? DoctorSchedules { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
