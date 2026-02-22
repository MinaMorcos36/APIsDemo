using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Services.Implementations
{
    public class PostSaveService : IPostSaveService
    {
        private readonly AppDbContext _context;

        public PostSaveService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleSaveAsync(int postId, int authorId, string authorType)
        {
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
