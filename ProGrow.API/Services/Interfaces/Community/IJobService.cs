using ProGrow.API.DTOs.Community.Jobs;
using Microsoft.AspNetCore.Http;

namespace ProGrow.API.Services.Interfaces.Community
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateAsync(CreateJobDto dto);
        Task<List<JobFeedDto>> GetFeedAsync();
        Task<List<CompanysJobDto>> GetJobsAsync(string? filter = null);
        Task ApplyAsync(int jobId, ApplyJobDto dto, IFormFile cvFile);
        Task<List<JobApplicationDto>> GetApplicationsAsync(int jobId, string? filter = null, string? sort = null);
        Task<List<JobApplicationDto>> GetMyApplicationsAsync(string? filter = null);
        Task<(byte[] Content, string FileName, string ContentType)> GetApplicationCvFileAsync(int applicationId);
        Task ApproveApplicationAsync(int applicationId);
        Task DeclineApplicationAsync(int applicationId);
        Task<JobResponseDto> SetActiveAsync(int jobId, bool isActive);
    }
}
