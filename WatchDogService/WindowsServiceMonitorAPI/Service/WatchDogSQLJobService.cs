using AutoMapper;
using WatchDogServiceApi.Dto;
using WatchDogServiceApi.Entities;
using WatchDogServiceApi.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WatchDogServiceApi.Service
{
    public class WatchDogSQLJobService : IWatchDogSQLJobService
    {
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;
        private readonly IWatchDogSQLJobsRepository _watchDogSQLJobRepository;

        // Constructor accepts the repository, mapper, and logging service
        public WatchDogSQLJobService(IWatchDogSQLJobsRepository watchDogSQLJobsRepository, IMapper mapper, ILoggingService loggingService)
        {
            _watchDogSQLJobRepository = watchDogSQLJobsRepository;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        // Method to get all jobs asynchronously
        public async Task<IEnumerable<AdminWatchDogSQLJobDto>> GetAllSqlJobsAsync()
        {
            try
            {
                // Use the repository to get all jobs
                var jobs = await _watchDogSQLJobRepository.GetAllAsync(); // Assuming GetAllAsync() is part of the repository
                return _mapper.Map<IEnumerable<AdminWatchDogSQLJobDto>>(jobs);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error retrieving all SQL jobs", ex, "Service");
                throw;
            }
        }

        // Method to get a job by ID asynchronously
        public async Task<AdminWatchDogSQLJobDto> GetSqlJobByIdAsync(int id)
        {
            try
            {
                // Use the repository to get a job by ID
                var job = await _watchDogSQLJobRepository.GetByIdAsync(id); // Assuming GetByIdAsync() is part of the repository
                return _mapper.Map<AdminWatchDogSQLJobDto>(job);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error retrieving SQL job by ID in GetJobByIdAsync for ID {id}", ex, "Service");
                throw;
            }
        }

        // Method to add a job asynchronously
        public async Task AddSqlJobAsync(AdminWatchDogSQLJobDto jobDto)
        {
            try
            {
                var job = _mapper.Map<AdminWatchDogSQLJobs>(jobDto);
                await _watchDogSQLJobRepository.AddAsync(job); // Add via the repository
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error adding SQL job", ex, "Service");
                throw;
            }
        }

        // Method to update a job asynchronously
        public async Task UpdateSqlJobAsync(AdminWatchDogSQLJobDto jobDto)
        {
            try
            {
                // Fetch the existing entity from the database
                var existingSqlJob = await _watchDogSQLJobRepository.GetByIdAsync(jobDto.Pid);
                if (existingSqlJob == null)
                {
                    throw new InvalidOperationException("Sql job not found in the database.");
                }

                // Update the existing entity instead of creating a new one
                _mapper.Map(jobDto, existingSqlJob); // Map only changed properties

                // Save changes
                await _watchDogSQLJobRepository.UpdateAsync(existingSqlJob);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error updating Sql job service in UpdateSqlJobAsync", ex, "Service");
                throw;
            }

        }

        // Method to delete a job asynchronously
        public async Task DeleteSqlJobAsync(int id)
        {
            try
            {
                await _watchDogSQLJobRepository.DeleteAsync(id); // Delete via the repository
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error deleting SQL job in DeleteJobAsync for ID {id}", ex, "Service");
                throw;
            }
        }
    }
}
