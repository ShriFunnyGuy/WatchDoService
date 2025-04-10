using WatchDogServiceApi.Dto;

namespace WatchDogServiceApi.Service
{
    public interface IWatchDogSQLJobService
    {
        Task<IEnumerable<AdminWatchDogSQLJobDto>> GetAllSqlJobsAsync();
        Task<AdminWatchDogSQLJobDto> GetSqlJobByIdAsync(int id);
        Task AddSqlJobAsync(AdminWatchDogSQLJobDto jobDto);
        Task UpdateSqlJobAsync(AdminWatchDogSQLJobDto jobDto);
        Task DeleteSqlJobAsync(int id);
    }
}
