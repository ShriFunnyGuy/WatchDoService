namespace WatchDogServiceApi.Service
{
    public interface ILoggingService
    {
        Task LogErrorAsync(string message, Exception exception, string source);
        Task LogInfoAsync(string message, string source);
    }
}
