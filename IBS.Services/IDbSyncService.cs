using System.Threading.Tasks;

namespace IBS.Services
{
    public interface IDbSyncService
    {
        Task SyncAsync(string sourceConnectionString, CancellationToken cancellationToken = default);
    }
}
