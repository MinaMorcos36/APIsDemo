using APIsDemo.DTOs.Community.Jobs;

namespace APIsDemo.Services.Interfaces
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateAsync(CreateJobDto dto, int userId);
    }
}
