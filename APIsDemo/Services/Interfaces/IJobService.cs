using APIsDemo.DTOs.Jobs;

namespace APIsDemo.Services.Interfaces
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateAsync(CreateJobDto dto);
    }
}