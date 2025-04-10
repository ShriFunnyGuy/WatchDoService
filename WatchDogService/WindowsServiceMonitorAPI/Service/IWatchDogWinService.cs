using WatchDogServiceApi.Dto;

namespace WatchDogServiceApi.Service
{
    public interface IWatchDogWinService
    {
        Task<IEnumerable<AdminWatchDogWinServicesDto>> GetAllWinServicesAsync();
        Task<AdminWatchDogWinServicesDto> GetWinServiceByIdAsync(int id);
        Task AddWinServiceAsync(AdminWatchDogWinServicesDto serviceDto);
        Task UpdateWinServiceAsync(AdminWatchDogWinServicesDto serviceDto);
        Task DeleteWinServiceAsync(int id);
    }
}
