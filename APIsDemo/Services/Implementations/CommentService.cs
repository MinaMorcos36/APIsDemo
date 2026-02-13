using APIsDemo.DTOs.Community.Comments;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;
        public CommentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CommentResponseDto> CreateAsync(int postId, int authorId, string authorType, CreateCommentDto dto)
        {
            // Optional safety check
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                throw new Exception("Post not found");

            // Optional reply validation
            if (dto.ParentCommentId != null)
            {
                var parentExists = await _context.Comments
                    .AnyAsync(c => c.Id == dto.ParentCommentId && c.PostId == postId);

                if (!parentExists)
                    throw new Exception("Invalid parent comment");
            }

            var comment = new Comment
            {
                PostId = postId,
                AuthorId = authorId,
                AuthorType = authorType,
                ParentCommentId = dto.ParentCommentId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return new CommentResponseDto
            {
                Id = comment.Id,
                AuthorId = authorId,
                AuthorType = authorType,
                PostId = postId,
                ParentCommentId = comment.ParentCommentId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<IEnumerable<CommentDto>> GetByPostIdAsync(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .Include(c => c.InverseParentComment)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                AuthorId = c.AuthorId,
                AuthorType = c.AuthorType,
                CreatedAt = c.CreatedAt,
                Replies = c.InverseParentComment.Select(r => new CommentDto
                {
                    Id = r.Id,
                    Content = r.Content,
                    AuthorId = r.AuthorId,
                    AuthorType = r.AuthorType,
                    CreatedAt = r.CreatedAt
                }).ToList()
            });
        }
    }
}
