using Microsoft.EntityFrameworkCore;
using WatchDogServiceApi.Entities;
using WatchDogServiceApi.Service;

namespace WatchDogServiceApi.Repository
{
    public class WatchDogWinServicesRepository : BaseRepository<AdminWatchDogWinServices>, IWatchDogWinServicesRepository
    {
        public WatchDogWinServicesRepository(ApplicationDbContext context) : base(context) { }
    }    
}
