using APIsDemo.DTOs.Community.Jobs;

namespace APIsDemo.Services.Interfaces.Community
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateAsync(CreateJobDto dto);
        Task<List<JobFeedDto>> GetFeedAsync();
        Task<List<CompanysJobDto>> GetJobsAsync(string? filter = null);
        Task ApplyAsync(int jobId);
        Task<List<JobApplicationDto>> GetApplicationsAsync(int? jobId = null, string? filter = null);
        Task<List<JobApplicationDto>> GetMyApplicationsAsync(string? filter = null);
        Task ApproveApplicationAsync(int applicationId);
        Task DeclineApplicationAsync(int applicationId);
        Task<JobResponseDto> SetActiveAsync(int jobId, bool isActive);
    }
}