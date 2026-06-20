using ProGrow.API.DTOs.Community.Jobs;
using Microsoft.AspNetCore.Http;

namespace ProGrow.API.Services.Interfaces.Community
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateAsync(CreateJobDto dto, IFormFile bannerImage);
        Task<List<JobFeedDto>> GetFeedAsync(int? page = null, int? pageSize = null);
        Task<List<CompanysJobDto>> GetJobsAsync(string? filter = null, int? page = null, int? pageSize = null);
        Task<List<JobCategoryDto>> GetJobCategoriesAsync();
        Task<List<JobTypeDto>> GetJobTypesAsync();
        Task<List<LocationModeDto>> GetLocationModesAsync();
        Task ApplyAsync(int jobId, ApplyJobDto dto, IFormFile cvFile);
        Task<List<JobApplicationDto>> GetApplicationsAsync(int jobId, string? sort = null, int? page = null, int? pageSize = null);
        Task<List<JobApplicationDto>> GetMyApplicationsAsync(string? filter = null, int? page = null, int? pageSize = null);
        Task<(byte[] Content, string FileName, string ContentType)> GetApplicationCvFileAsync(int applicationId);
        Task AcceptApplicationAsync(int applicationId);
        Task RejectApplicationAsync(int applicationId);
        Task<JobResponseDto> SetActiveAsync(int jobId, bool isActive);
        Task<JobDetailsDto> GetJobDetailsAsync(int jobId);
    }
}
