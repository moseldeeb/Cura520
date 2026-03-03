using Cura520.DataAccess;
using Cura520.Models;

namespace Cura520.Repos
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        private readonly ApplicationDbContext _context; //= new ApplicationDbContext();
        public AppointmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddRange(IEnumerable<Appointment> appointments, CancellationToken cancellationToken = default)
        {
            await _context.Appointments.AddRangeAsync(appointments, cancellationToken);
        }

    }
}
