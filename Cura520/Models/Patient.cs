namespace Cura520.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string BloodType { get; set; }
        public string Allergies { get; set; }
        public MedicalHistory MedicalHistory { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; }

        public bool IsDeleted { get; set; } = false;


    }
}
