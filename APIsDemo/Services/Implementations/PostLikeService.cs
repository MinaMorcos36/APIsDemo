using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Services.Implementations
{
    public class PostLikeService : IPostLikeService
    {
        private readonly AppDbContext _context;

        public PostLikeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleLikeAsync(int postId, int authorId, string authorType)
        {
            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.AuthorId == authorId);

            if (existingLike != null)
            {
                _context.PostLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false;
            }

            var like = new PostLike
            {
                PostId = postId,
                AuthorId = authorId,
                AuthorType = authorType,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
