using System.ComponentModel.DataAnnotations;

namespace Cura520.Models
{
    public class PrescriptionItem
    {
        public int id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public int Duration { get; set; }
        public int prescriptionId { get; set; }
        public Prescription Prescription { get; set; }
    }
}
