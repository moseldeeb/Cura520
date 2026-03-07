 using Cura520.Models  ;
namespace Cura520.ViewModel
{
    public class DoctorVM
    {
        public List<Doctor> Doctors { get; set; }
        public string Specialty { get; set; } 
        public ApplicationUser ApplicationUser { get; set; } 
    }
}
