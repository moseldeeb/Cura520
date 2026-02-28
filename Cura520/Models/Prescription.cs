namespace Cura520.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public DateTime IssuedDate { get; set; } = DateTime.Now;
        public string Note { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public int? MedicalHistoryId { get; set; }
        public MedicalHistory MedicalHistory { get; set; }
        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}
