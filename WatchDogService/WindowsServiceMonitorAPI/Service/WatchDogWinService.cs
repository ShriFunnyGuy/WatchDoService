using AutoMapper;
using WatchDogServiceApi.Dto;
using WatchDogServiceApi.Entities;
using WatchDogServiceApi.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WatchDogServiceApi.Service
{
    public class WatchDogWinService : IWatchDogWinService
    {
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;
        private readonly IWatchDogWinServicesRepository _watchDogWinServicesRepository;

        // Constructor accepts the repository, mapper, and logging service
        public WatchDogWinService(IWatchDogWinServicesRepository watchDogWinServicesRepository, IMapper mapper, ILoggingService loggingService)
        {
            _watchDogWinServicesRepository = watchDogWinServicesRepository;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        public async Task<IEnumerable<AdminWatchDogWinServicesDto>> GetAllWinServicesAsync()
        {
            try
            {
                // Use the repository to get all services
                var services = await _watchDogWinServicesRepository.GetAllAsync(); // Assuming GetAllAsync() is part of the BaseRepository
                return _mapper.Map<IEnumerable<AdminWatchDogWinServicesDto>>(services);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error retrieving all Windows services in GetAllWinServicesAsync", ex, "Service");
                throw;
            }
        }

        public async Task<AdminWatchDogWinServicesDto> GetWinServiceByIdAsync(int id)
        {
            try
            {
                // Use the repository to get a service by ID
                var service = await _watchDogWinServicesRepository.GetByIdAsync(id); // Assuming GetByIdAsync() is part of the BaseRepository
                return _mapper.Map<AdminWatchDogWinServicesDto>(service);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error retrieving Windows service by ID in GetWinServiceByIdAsync for ID {id}", ex, "Service");
                throw;
            }
        }

        public async Task AddWinServiceAsync(AdminWatchDogWinServicesDto serviceDto)
        {
            try
            {
                var service = _mapper.Map<AdminWatchDogWinServices>(serviceDto);
                await _watchDogWinServicesRepository.AddAsync(service); // Add via the repository
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error adding Windows service in AddWinServiceAsync", ex, "Service");
                throw;
            }
        }

        public async Task UpdateWinServiceAsync(AdminWatchDogWinServicesDto serviceDto)
        {
            try
            {
                // Fetch the existing entity from the database
                var existingService = await _watchDogWinServicesRepository.GetByIdAsync(serviceDto.Pid);
                if (existingService == null)
                {
                    throw new InvalidOperationException("Service not found in the database.");
                }

                // Update the existing entity instead of creating a new one
                _mapper.Map(serviceDto, existingService); // Map only changed properties

                // Save changes
                await _watchDogWinServicesRepository.UpdateAsync(existingService);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error updating Windows service in UpdateWinServiceAsync", ex, "Service");
                throw;
            }
        }

        public async Task DeleteWinServiceAsync(int id)
        {
            try
            {
                // Use the repository to delete the service by ID
                await _watchDogWinServicesRepository.DeleteAsync(id); // Delete via the repository
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error deleting Windows service in DeleteWinServiceAsync for ID {id}", ex, "Service");
                throw;
            }
        }
    }
}
