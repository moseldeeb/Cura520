using System.ComponentModel.DataAnnotations;

namespace Cura520.ViewModel.Patient
{
    public class CreateAppointmentVM
    {
        [Required(ErrorMessage = "Symptom summary is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Symptom summary must be between 10 and 1000 characters")]
        [Display(Name = "Describe Your Symptoms")]
        public string SymptomSummary { get; set; }

        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Preferred Appointment Date")]
        [FutureDate(ErrorMessage = "Appointment date must be in the future")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Doctor is required")]
        [Display(Name = "Select a Doctor")]
        public int DoctorId { get; set; }

        [Display(Name = "Doctor")]
        public string DoctorName { get; set; }

        [Display(Name = "Doctor Specialty")]
        public string DoctorSpecialty { get; set; }

        [Display(Name = "Consultation Fee")]
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Patient Name")]
        public string PatientName { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        public int PatientId { get; set; }
    }

    /// <summary>
    /// Custom validation attribute to ensure appointment date is in the future
    /// </summary>
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            if (value is DateTime dateTime)
            {
                return dateTime > DateTime.Now;
            }

            return false;
        }
    }
}
