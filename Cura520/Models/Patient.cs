namespace Cura520.Models
{
    public class Patient
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Img { get; set; } = "defaultImg.png";

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        [Phone]
        public string PhoneNumber { get; set; }
        public string BloodType { get; set; }
        public string Allergies { get; set; }
        public ICollection<MedicalHistory> MedicalHistories { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; }

        public bool IsDeleted { get; set; } = false;


    }
}
