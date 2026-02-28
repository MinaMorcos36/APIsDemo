using APIsDemo.DTOs.Community.Jobs;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces;

namespace APIsDemo.Services.Implementations
{
    public class JobService: IJobService
    {
        private readonly AppDbContext _context;
        public JobService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobResponseDto> CreateAsync(CreateJobDto dto, int authorId)
        {
            var job = new Job
            {
                CompanyId = authorId,
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                CreatedAt = DateTime.UtcNow
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return new JobResponseDto
            {
                Id = job.Id,
                AuthorId = job.CompanyId,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                CreatedAt = (DateTime)job.CreatedAt
            };
        }
    }
}
