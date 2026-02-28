namespace Cura520.Models
{
    public class MedicalHistory
    {
        public int Id { get; set; }
        public string Diagnosis { get; set; }
        public string Note { get; set; }
        public DateTime RecordDate { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public string ChronicDiseases { get; set; }
        public int? AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
        public List<Prescription> Prescriptions { get; set; }

    }
}
