using System.ComponentModel.DataAnnotations;

namespace Cura520.Models
{
    public enum Status
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled
    }
    public class Appointment
    {
        public int Id { get; set; }
        [Required, MaxLength(1000)]
        public string SymptomSummary { get; set; }
        public DateTime AppointmentDate { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        public Invoice Invoice { get; set; }

        public bool IsDeleted { get; set; } = false;


    }
}
