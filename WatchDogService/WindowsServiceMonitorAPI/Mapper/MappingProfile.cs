using AutoMapper;
using WatchDogServiceApi.Dto;
using WatchDogServiceApi.Entities;

namespace WatchDogServiceApi.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AdminWatchDogSQLJobs, AdminWatchDogSQLJobDto>();
            CreateMap<AdminWatchDogSQLJobDto, AdminWatchDogSQLJobs>();
            CreateMap<AdminWatchDogWinServices, AdminWatchDogWinServicesDto>();
            CreateMap<AdminWatchDogWinServicesDto, AdminWatchDogWinServices>();
        }
    }
}
