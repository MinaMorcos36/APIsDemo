using APIsDemo.DTOs.Community.Jobs;

namespace APIsDemo.Services.Interfaces.Community
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateAsync(CreateJobDto dto);
        Task<List<JobFeedDto>> GetFeedAsync();
    }
}