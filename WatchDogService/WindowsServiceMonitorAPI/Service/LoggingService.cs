namespace WatchDogServiceApi.Service
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public async Task LogErrorAsync(string message, Exception exception, string source)
        {
            _logger.LogError($"Source: {source} - {message} - Exception: {exception.Message}");
            await Task.CompletedTask;
        }

        public async Task LogInfoAsync(string message, string source)
        {
            _logger.LogInformation($"Source: {source} - {message}");
            await Task.CompletedTask;
        }
    }

}
