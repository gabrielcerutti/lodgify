using Lodgify.Api.Database.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Lodgify.Api.Database.Repositories.Abstractions
{
    public interface IAuditoriumsRepository
    {
        Task<AuditoriumEntity> GetAsync(int auditoriumId, CancellationToken cancel);
    }
}