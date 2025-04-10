using System.Text.Json;
using WatchDogService.Entities;

namespace WatchDogService.ApiService
{
    public class WatchDogWinServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WatchDogWinServiceClient> _logger;
        private readonly string _baseUrl;

        public WatchDogWinServiceClient(HttpClient httpClient, ILogger<WatchDogWinServiceClient> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Read Base URL from configuration
            _baseUrl = config.GetValue<string>("WinAppSettings:MonitorServiceApi:BaseUrl");

            // Validate configuration value
            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                throw new ArgumentNullException("MonitorServiceApi BaseUrl is missing or invalid in configuration.");
            }
        }

        /// <summary>
        /// Fetches the list of Windows services from the API for a given server name.
        /// </summary>
        /// <param name="serverName">The name of the server to fetch services for.</param>
        /// <returns>An instance of ApiResponseModel containing the list of services.</returns>
        public async Task<ApiResponseModel<IEnumerable<WatchDogWinServices>>> GetWinServicesByServerName(string serverName)
        {
            // Ensure serverName is not null or empty
            if (string.IsNullOrWhiteSpace(serverName))
            {
                _logger.LogWarning("Server name was null or empty.");
                return new ApiResponseModel<IEnumerable<WatchDogWinServices>>
                {
                    Message = "Server name is required",
                    Result = false
                };
            }

            // Construct API URL with serverName as a query parameter (safely encoded)
            string apiUrl = $"{_baseUrl}/WatchDogWinServices/GetWinServicesByServername/{Uri.EscapeDataString(serverName)}";

            _logger.LogInformation($"Fetching services from API: {apiUrl}");

            try
            {
                // Make asynchronous GET request
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode(); // Throws if status code is not 2xx

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response into ApiResponseModel
                var result = JsonSerializer.Deserialize<ApiResponseModel<IEnumerable<WatchDogWinServices>>>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogWarning("API response is null or invalid.");
                    return new ApiResponseModel<IEnumerable<WatchDogWinServices>>
                    {
                        Message = "Invalid response from API",
                        Result = false
                    };
                }

                return result;
            }
            catch (HttpRequestException httpEx)
            {
                // Handle HTTP request-specific exceptions (like 404, 500, connection issues)
                _logger.LogError(httpEx, "HTTP error occurred while fetching services from API.");
                return new ApiResponseModel<IEnumerable<WatchDogWinServices>>
                {
                    Message = "HTTP error while fetching services",
                    Result = false
                };
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                _logger.LogError(ex, "Error fetching services from API.");
                return new ApiResponseModel<IEnumerable<WatchDogWinServices>>
                {
                    Message = "Error fetching services",
                    Result = false,
                    Data = null
                };
            }
        }
    }
}
