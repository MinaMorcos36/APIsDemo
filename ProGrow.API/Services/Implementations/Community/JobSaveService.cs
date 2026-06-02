using Microsoft.EntityFrameworkCore;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Community;
using System.Security.Claims;

namespace ProGrow.API.Services.Implementations.Community
{
    public class JobSaveService : IJobSaveService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JobSaveService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public async Task<bool> ToggleSaveAsync(int jobId)
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var existingSave = await _context.JobSaves
                .FirstOrDefaultAsync(js => js.JobId == jobId && js.AuthorId == authorId && js.AuthorType == authorType);

            if (existingSave != null)
            {
                _context.JobSaves.Remove(existingSave);
                await _context.SaveChangesAsync();
                return false;
            }

            var save = new JobSave
            {
                JobId = jobId,
                AuthorId = authorId,
                AuthorType = authorType,
                SavedAt = DateTime.UtcNow
            };

            _context.JobSaves.Add(save);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
