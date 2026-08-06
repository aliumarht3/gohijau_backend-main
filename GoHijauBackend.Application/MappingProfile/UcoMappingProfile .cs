using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;
using AutoMapper;
namespace GoHijauBackend.Application.MappingProfile
{
    public sealed class UcoMappingProfile : Profile
    {
        public UcoMappingProfile()
        {
            CreateMap<MachineUCOTracking, MachineUCOTrackingDTO>();
        }
    }
}
