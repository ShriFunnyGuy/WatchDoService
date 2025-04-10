
using WatchDogServiceApi.Entities;

namespace WatchDogServiceApi.Repository
{
    public class WatchDogServicesRepository : BaseRepository<AdminWatchDogSQLJobs>, IWatchDogSQLJobsRepository
    {
        public WatchDogServicesRepository(ApplicationDbContext context) : base(context) { }
    }
}
