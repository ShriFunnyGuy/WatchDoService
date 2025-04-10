
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text.Json;
using Serilog;
using WatchDogService.ApiService;

namespace WatchDogService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly WatchDogWinServiceClient _monitorWinServiceClient;
        private Config _config;
        enum ServiceStatus { Healthy, Stopped, Missing }
        private readonly Dictionary<string, string> _lastKnownServiceStatus = new();
        private DateTime _lastEmailSentDate = DateTime.MinValue;

        public Worker(ILogger<Worker> logger, WatchDogWinServiceClient monitorWinServiceClient)
        {
            _logger = logger;
            _monitorWinServiceClient = monitorWinServiceClient;
            _config = LoadConfig(); // Load configuration from file
            ConfigureLogging();     // Set up Serilog
        }

        private void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/service_monitor.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckServicesAsync(stoppingToken); // Periodically check services
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking services");
                    Log.Error(ex, "Error checking services");
                }

                await Task.Delay(TimeSpan.FromSeconds(_config.WinAppSettings.CheckIntervalSeconds), stoppingToken);
            }
        }

        private Config LoadConfig()
        {
            try
            {
                string json = File.ReadAllText("appsettings.json");
                return JsonSerializer.Deserialize<Config>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading config");
                Log.Error(ex, "Error reading config");
                return new Config(); // Return empty config as fallback
            }
        }

        private string GetServiceDisplayName(string serviceName)
        {
            try
            {
                using var service = new ServiceController(serviceName);
                return service.DisplayName;
            }
            catch
            {
                return serviceName; // Fallback if display name is unavailable
            }
        }

        private async Task CheckServicesAsync(CancellationToken stoppingToken)
        {
            var allRunningServices = new List<string>();
            var allStoppedServices = new List<string>();
            var allMissingServices = new List<string>();

            var stoppedServices = new List<string>();
            var missingServices = new List<string>();
            var healthyServices = new List<string>();

            // Get current machine name for API call
            string serverName = Environment.MachineName;

            // Call API to get expected services
            var response = await _monitorWinServiceClient.GetWinServicesByServerName(serverName);

            if (response.Result)
            {
                var allServicesList = response.Data;

                var currentConfiguredServices = new HashSet<string>();

                foreach (var serviceDetails in allServicesList)
                {
                    if (string.IsNullOrWhiteSpace(serviceDetails.ServiceName))
                    {
                        _logger.LogWarning("Skipping invalid service name.");
                        continue;
                    }

                    currentConfiguredServices.Add(serviceDetails.ServiceName);

                    try
                    {
                        using var service = new ServiceController(serviceDetails.ServiceName);
                        var status = service.Status;

                        if (status == ServiceControllerStatus.Running)
                        {
                            allRunningServices.Add(serviceDetails.ServiceName);
                            if (!_lastKnownServiceStatus.TryGetValue(serviceDetails.ServiceName, out var lastStatus) || lastStatus != ServiceStatus.Healthy.ToString())
                            {
                                healthyServices.Add(serviceDetails.ServiceName);
                                _lastKnownServiceStatus[serviceDetails.ServiceName] = ServiceStatus.Healthy.ToString();
                            }
                        }
                        else
                        {
                            allStoppedServices.Add(serviceDetails.ServiceName);
                            if (!_lastKnownServiceStatus.TryGetValue(serviceDetails.ServiceName, out var lastStatus) || lastStatus != ServiceStatus.Stopped.ToString())
                            {
                                stoppedServices.Add(serviceDetails.ServiceName);
                                _lastKnownServiceStatus[serviceDetails.ServiceName] = ServiceStatus.Stopped.ToString();
                            }
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        allMissingServices.Add(serviceDetails.ServiceName);
                        //missingServices.Add(serviceDetails.ServiceName);

                        if (!_lastKnownServiceStatus.TryGetValue(serviceDetails.ServiceName, out var lastStatus) || lastStatus != ServiceStatus.Missing.ToString())
                        {
                            missingServices.Add(serviceDetails.ServiceName);
                            _lastKnownServiceStatus[serviceDetails.ServiceName] = ServiceStatus.Missing.ToString();
                        }
                    }

                    catch (UnauthorizedAccessException)
                    {
                        _logger.LogError($"Access denied for service: {serviceDetails.ServiceName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error checking service: {serviceDetails.ServiceName}");
                    }
                }

                // Remove services that no longer exist in the Database
                var previousServices = _lastKnownServiceStatus.Keys.ToList();
                foreach (var serviceName in previousServices)
                {
                    if (!currentConfiguredServices.Contains(serviceName))
                    {
                        _logger.LogInformation($"Removing stale service entry from known status: {serviceName}");
                        _lastKnownServiceStatus.Remove(serviceName);
                    }
                }

                var today = DateTime.Today;

                // Check if there are any new missing or stopped services, or if a missing service was restored
                bool hasNewStoppedOrMissing = stoppedServices.Any() || missingServices.Any();
                bool hasAnyStoppedOrMissingNow = allStoppedServices.Any() || allMissingServices.Any();

                // If there are new issues or status change (service restored), trigger email alert
                if (hasNewStoppedOrMissing || (_lastEmailSentDate.Date < today && hasAnyStoppedOrMissingNow))
                {
                    SendEmailAlert(allStoppedServices, allMissingServices);
                    _lastEmailSentDate = today;
                }

                // Log full status of all services
                _logger.LogInformation("📊 Full Service Status Snapshot:");
                _logger.LogInformation("   ✅ Running Services: {Count} - {Names}", allRunningServices.Count, string.Join(", ", allRunningServices));
                _logger.LogInformation("   ❌ Stopped Services: {Count} - {Names}", allStoppedServices.Count, string.Join(", ", allStoppedServices));
                _logger.LogInformation("   ❓ Missing Services: {Count} - {Names}", allMissingServices.Count, string.Join(", ", allMissingServices));
            }
            else
            {
                _logger.LogError("Failed to retrieve services from API. Error: {Message}", response?.Message ?? "Unknown");
            }
        }

        private void SendEmailAlert(List<string> stoppedServices, List<string> missingServices)
        {
            try
            {
                var smtp = _config.WinAppSettings.SMTP;

                using var client = new SmtpClient(smtp.Server, smtp.Port)
                {
                    Credentials = new NetworkCredential(smtp.Username, smtp.Password),
                    EnableSsl = smtp.UseSSL
                };

                string serverName = Environment.MachineName;
                string timeStamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

                var subject = $"🚨 WatchDog Windows Services Alert - {serverName}";

                var body = $@"
<html>
    <body style='font-family:Segoe UI, sans-serif; color:#333;'>
        <h2 style='color:#d9534f;'>🚨 WatchDog Service Alert</h2>
        <p>
            <strong>Server:</strong> {serverName}<br/>
            <strong>Timestamp:</strong> {timeStamp}
        </p>";

                // Only include Missing Services if any exist
                if (missingServices?.Any() == true)
                {
                    body += $"<h3 style='color:#f0ad4e;'>❌ Missing / Not Found Service{(missingServices.Count == 1 ? "" : "s")}</h3><ul>";
                    foreach (var serviceName in missingServices)
                    {
                        body += $"<li>{GetServiceDisplayName(serviceName)} (<code>{WebUtility.HtmlEncode(serviceName)}</code>)</li>";
                    }
                    body += "</ul>";
                }

                // Only include Stopped Services if any exist
                if (stoppedServices?.Any() == true)
                {
                    body += $"<h3 style='color:#f0ad4e;'>🛑 Stopped Service{(stoppedServices.Count == 1 ? "" : "s")}</h3><ul>";
                    foreach (var serviceName in stoppedServices)
                    {
                        body += $"<li>{GetServiceDisplayName(serviceName)} (<code>{WebUtility.HtmlEncode(serviceName)}</code>)</li>";
                    }
                    body += "</ul>";
                }

                body += @"
        <p style='margin-top:20px;'>Please check and take the necessary actions.</p>
        <p>Best Regards,<br/>🚀 WatchDog Service</p>
    </body>
</html>";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtp.FromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                foreach (var to in smtp.ToEmail.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    mailMessage.To.Add(to.Trim());
                }

                client.Send(mailMessage);
                _logger.LogInformation("Alert email sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send alert email.");
            }
        }

        //private float GetCpuUsage()
        //{
        //    try
        //    {
        //        using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
        //        {
        //            return cpuCounter.NextValue();  // This gives the CPU usage in percentage.
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error fetching CPU usage.");
        //        return 0;
        //    }
        //}

        //private float GetDiskSpaceUsage(string driveLetter)
        //{
        //    try
        //    {
        //        var drive = new DriveInfo(driveLetter);
        //        var totalSpace = drive.TotalSize;
        //        var freeSpace = drive.AvailableFreeSpace;
        //        return (float)(freeSpace / (double)totalSpace) * 100;  // Returns percentage of free space.
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error fetching disk space for drive {driveLetter}:");
        //        return 0;
        //    }
        //}

        //private float GetRamUsage()
        //{
        //    try
        //    {
        //        using (var ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use"))
        //        {
        //            return ramCounter.NextValue();  // This returns the percentage of used RAM.
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error fetching RAM usage.");
        //        return 0;
        //    }
        //}

    }

    public record Config(
    WinAppSettings WinAppSettings
);

    public record WinAppSettings(
        string[] ServiceNames,
        int CheckIntervalSeconds,
        SMTPConfig SMTP
    )
    {
        public WinAppSettings(string[] ServiceNames, SMTPConfig SMTP)
            : this(ServiceNames, 60, SMTP) { }
    }

    public record SMTPConfig(
        string Server,
        int Port,
        bool UseSSL,
        string Username,
        string Password,
        string FromEmail,
        string ToEmail
    );
}
