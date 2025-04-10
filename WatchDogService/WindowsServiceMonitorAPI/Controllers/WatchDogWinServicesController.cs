using Microsoft.AspNetCore.Mvc;
using WatchDogServiceApi.Dto;
using WatchDogServiceApi.Service;
using WatchDogServiceApi.Entities;

namespace WatchDogServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatchDogWinServicesController : BaseApiController
    {
        private readonly IWatchDogWinService _service;
        private readonly ILoggingService _loggingService;

        public WatchDogWinServicesController(IWatchDogWinService service, ILoggingService loggingService)
        {
            _service = service;
            _loggingService = loggingService;
        }

        // GET: api/WatchDogWinServices
        [HttpGet("GetAllWinServices")]
        [ActionName("GetAllWinServices")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]  // This will generate the correct response schema in Swagger
        public async Task<IActionResult> GetAllWinServices()
        {
            try
            {
                var services = await _service.GetAllWinServicesAsync();
                if (services == null || !services.Any())
                {
                    return this.ApiResponse<string>("No services found", false, ""); // Empty string as Data if no services
                }

                return this.ApiResponse<IEnumerable<AdminWatchDogWinServicesDto>>("Services retrieved successfully", true, services);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error while retrieving Windows services", ex, "Controller");
                return this.ApiResponse<string>("An error occurred while fetching services", false, "");
            }

        }

        // GET: api/WatchDogWinServices/{id}
        [HttpGet("GetWinServiceById/{id}")]
        [ActionName("GetWinServiceById")]        
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)] // 404 for not found
        public async Task<IActionResult> GetWinServiceById(int id)
        {
            try
            {


                var service = await _service.GetWinServiceByIdAsync(id);
                if (service == null)
                {
                    return this.ApiResponse<object>("Service not found", false, null);
                }
                return this.ApiResponse<AdminWatchDogWinServicesDto>("Service retrieved successfully", true, service);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error while retrieving Windows service by ID", ex, "Controller");
                return this.ApiResponse<string>("An error occurred while fetching the service", false, "");
            }
        }

        // POST: api/WatchDogWinServices
        [HttpPost("CreateWinService")]
        [ActionName("CreateWinService")]        
        [ProducesResponseType(typeof(ApiResponse<string>), 201)]  // 201 Created
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]  // 400 for invalid data
        public async Task<IActionResult> CreateWinService([FromBody] AdminWatchDogWinServicesDto serviceDto)
        {
            try
            {


                if (serviceDto == null)
                {
                    return this.ApiResponse<object>("Invalid data", false, null); // Handle invalid data
                }

                await _service.AddWinServiceAsync(serviceDto);

                return CreatedAtAction(nameof(GetWinServiceById), new { id = serviceDto.Pid },
                    this.ApiResponse<AdminWatchDogWinServicesDto>("Service added successfully", true, serviceDto));

            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error while adding Windows service", ex, "Controller");
                return this.ApiResponse<string>("An error occurred while adding the service", false, "");
            }
        }

        // PUT: api/WatchDogWinServices/{id}
        [HttpPut("UpdateWinService/{id}")]
        [ActionName("UpdateWinService")]        
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]  // 400 for invalid data
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]  // 404 for not found
        public async Task<IActionResult> UpdateWinService(int id, [FromBody] AdminWatchDogWinServicesDto serviceDto)
        {
            try
            {
                if (serviceDto == null)
                {
                    return this.ApiResponse<object>("Invalid data", false, null);
                }

                if (id != serviceDto.Pid)
                {
                    return this.ApiResponse<object>("Service ID mismatch", false, null); // Handle ID mismatch
                }

                var existingService = await _service.GetWinServiceByIdAsync(id);
                if (existingService == null)
                {
                    return this.ApiResponse<object>("Service not found", false, null);
                }

                await _service.UpdateWinServiceAsync(serviceDto);
                return this.ApiResponse<AdminWatchDogWinServicesDto>("Service updated successfully", true, serviceDto);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error while updating Windows service", ex, "Controller");
                return this.ApiResponse<string>("An error occurred while updating the service", false, "");
            }
        }

        // DELETE: api/WatchDogWinServices/{id}
        [HttpDelete("DeleteWinService/{id}")]
        [ActionName("DeleteWinService")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]  // 404 for not found
        public async Task<IActionResult> DeleteWinService(int id)
        {
            try
            {
                var service = await _service.GetWinServiceByIdAsync(id);
                if (service == null)
                {
                    return this.ApiResponse<object>("Service not found", false, null);
                }

                await _service.DeleteWinServiceAsync(id);
                return this.ApiResponse<object>("Service deleted successfully", true, null);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error while deleting Windows service", ex, "Controller");
                return this.ApiResponse<string>("An error occurred while deleting the service", false, "");
            }
        }
    }
}
