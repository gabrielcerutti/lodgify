using Lodgify.Api.Database.Entities;
using Lodgify.Api.Database.Repositories.Abstractions;

namespace Lodgify.Api.Database.Repositories
{
    public class AuditoriumsRepository : IAuditoriumsRepository
    {
        private readonly CinemaContext _context;

        public AuditoriumsRepository(CinemaContext context)
        {
            _context = context;
        }

        public async Task<AuditoriumEntity> GetAsync(int auditoriumId, CancellationToken cancel)
        {
            return await _context.Auditoriums
                .Include(x => x.Seats)
                .FirstOrDefaultAsync(x => x.Id == auditoriumId, cancel);
        }
    }
}
