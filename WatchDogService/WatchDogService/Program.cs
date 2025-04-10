using WatchDogService;
using WatchDogService.ApiService;

var builder = Host.CreateApplicationBuilder(args);

// Register HttpClient for API service with a default base address
builder.Services.AddHttpClient<WatchDogWinServiceClient>(client =>
{
    // Optionally set a base address or default headers here if required
    // client.BaseAddress = new Uri("https://example.com/api/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Register the background worker service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
