using Cura520.Models;
using Microsoft.EntityFrameworkCore;

namespace Cura520.Repos
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task AddRange(IEnumerable<Appointment> appointments, CancellationToken cancellationToken = default);
    }
}
