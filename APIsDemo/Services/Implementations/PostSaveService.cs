using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIsDemo.Services.Implementations
{
    public class PostSaveService : IPostSaveService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostSaveService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public async Task<bool> ToggleSaveAsync(int postId)
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var existingSave = await _context.PostSaves
                .FirstOrDefaultAsync(ps => ps.PostId == postId && ps.AuthorId == authorId && ps.AuthorType == authorType);

            if (existingSave != null)
            {
                _context.PostSaves.Remove(existingSave);
                await _context.SaveChangesAsync();
                return false;
            }

            var save = new PostSave
            {
                PostId = postId,
                AuthorId = authorId,
                AuthorType = authorType,
                SavedAt = DateTime.UtcNow
            };

            _context.PostSaves.Add(save);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
