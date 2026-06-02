using Microsoft.EntityFrameworkCore;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Community;
using System.Security.Claims;

namespace ProGrow.API.Services.Implementations.Community
{
    public class JobLikeService : IJobLikeService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JobLikeService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetAuthorId()
        {
            return int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private string GetAuthorType()
        {
            return _httpContextAccessor.HttpContext!.User.FindFirstValue("AuthorType")!;
        }

        public async Task<bool> ToggleLikeAsync(int jobId)
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var existingLike = await _context.JobLikes
                .FirstOrDefaultAsync(jl => jl.JobId == jobId && jl.AuthorId == authorId && jl.AuthorType == authorType);

            if (existingLike != null)
            {
                _context.JobLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false;
            }

            var like = new JobLike
            {
                JobId = jobId,
                AuthorId = authorId,
                AuthorType = authorType,
                CreatedAt = DateTime.UtcNow
            };

            _context.JobLikes.Add(like);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
