namespace Cura520.ViewModel
{
    public class ScheduleVM
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
