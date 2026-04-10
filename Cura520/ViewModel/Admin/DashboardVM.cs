using Cura520.Models;

namespace Cura520.ViewModel.Admin
{
    public class DashboardVM
    {
        public IEnumerable<Models.Doctor> Doctors { get; set; }
        public IEnumerable<Models.Patient> Patients { get; set; }
        public IEnumerable<Appointment> Appointments { get; set; }
        public int DoctorCount { get; set; }
        public int PatientCount { get; set; }
        public int PendingAppointments { get; set; }
    }
}
