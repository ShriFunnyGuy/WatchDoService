using Microsoft.AspNetCore.Mvc;
using WatchDogServiceApi.Service;
using WatchDogServiceApi.Entities;
using WatchDogServiceApi.Dto;

namespace WatchDogServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatchDogSQLJobsController : BaseApiController
    {
        private readonly IWatchDogSQLJobService _service;
        private readonly ILoggingService _loggingService;

        public WatchDogSQLJobsController(IWatchDogSQLJobService service, ILoggingService loggingService)
        {
            _service = service;
            _loggingService = loggingService;
        }

        // GET: api/WatchDogSQLJobs
        [HttpGet("GetAllSqlJobs")]
        [ActionName("GetAllSqlJobs")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]  // This will generate the correct response schema in Swagger
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]  // 404 if no jobs found
        public async Task<IActionResult> GetAllSqlJobs()
        {
            try
            {
                var jobs = await _service.GetAllSqlJobsAsync();
                if (jobs == null || !jobs.Any())
                {
                    return this.ApiResponse<string>("No jobs found", false, "");  // Empty string if no jobs
                }

                return this.ApiResponse<IEnumerable<AdminWatchDogSQLJobDto>>("Jobs retrieved successfully", true, jobs);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error while retrieving SQL jobs", ex, "Controller");
                return this.ApiResponse<string>("An error occurred while fetching jobs", false, "");
            }
        }

        // GET: api/WatchDogSQLJobs/{id}
        [HttpGet("GetSqlJobById/{id}")]
        [ActionName("GetSqlJobById")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]  // 404 for not found
        public async Task<IActionResult> GetSqlJobById(int id)
        {
            try
            {


                var job = await _service.GetSqlJobByIdAsync(id);
                if (job == null)
                {
                    return this.ApiResponse<object>("Job not found", false, null);
                }
                return this.ApiResponse<AdminWatchDogSQLJobDto>("Job retrieved successfully", true, job);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error while retrieving SQL job by ID", ex, "Controller");
                return this.ApiResponse<string>("An error occurred while fetching the job", false, "");
            }
        }

        // POST: api/WatchDogSQLJobs
        [HttpPost("CreateSqlJob")]
        [ActionName("CreateSqlJob")]
        [ProducesResponseType(typeof(ApiResponse<string>), 201)]  // 201 Created
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]  // 400 for invalid data
        public async Task<IActionResult> CreateSqlJob([FromBody] AdminWatchDogSQLJobDto jobDto)
        {
            try
            {


                if (jobDto == null)
                {
                    return this.ApiResponse<object>("Invalid data", false, null); // Handle invalid data
                }

                await _service.AddSqlJobAsync(jobDto);
                return CreatedAtAction(nameof(GetSqlJobById), new { id = jobDto.Pid },
                    this.ApiResponse<AdminWatchDogSQLJobDto>("Job added successfully", true, jobDto));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error while adding SQL job", ex, "Controller");
                return this.ApiResponse<string>("An error occurred while adding the job", false, "");
            }
        }

        // PUT: api/WatchDogSQLJobs/{id}
        [HttpPut("UpdateSqlJob/{id}")]
        [ActionName("UpdateSqlJob")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]  // 400 for invalid data
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]  // 404 for not found
        public async Task<IActionResult> UpdateSqlJob(int id, [FromBody] AdminWatchDogSQLJobDto jobDto)
        {
            try
            {
                if (jobDto == null)
                {
                    return this.ApiResponse<object>("Invalid data", false, null);
                }

                if (id != jobDto.Pid)
                {
                    return this.ApiResponse<object>("Job ID mismatch", false, null);  // Handle ID mismatch
                }

                var existingJob = await _service.GetSqlJobByIdAsync(id);
                if (existingJob == null)
                {
                    return this.ApiResponse<object>("Job not found", false, null);
                }

                await _service.UpdateSqlJobAsync(jobDto);
                return this.ApiResponse<AdminWatchDogSQLJobDto>("Job updated successfully", true, jobDto);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error while updating SQL job", ex, "Controller");
                return this.ApiResponse<string>("An error occurred while updating the job", false, "");
            }
        }

        // DELETE: api/WatchDogSQLJobs/{id}
        [HttpDelete("DeleteSqlJob/{id}")]
        [ActionName("DeleteSqlJob")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]  // 404 for not found
        public async Task<IActionResult> DeleteSqlJob(int id)
        {
            try
            {
                var job = await _service.GetSqlJobByIdAsync(id);
                if (job == null)
                {
                    return this.ApiResponse<object>("Job not found", false, null);
                }

                await _service.DeleteSqlJobAsync(id);
                return this.ApiResponse<object>("Job deleted successfully", true, null);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error while deleting SQL job", ex, "Controller");
                return this.ApiResponse<string>("An error occurred while deleting the job", false, "");
            }
        }
    }
}
