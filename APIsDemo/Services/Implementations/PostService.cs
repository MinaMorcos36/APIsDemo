using APIsDemo.DTOs.Community.Posts;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;
        public PostService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PostResponseDto> CreateAsync(CreatePostDto dto, int authorId, string authorType)
        {
            var post = new Post
            {
                Content = dto.Content,
                AuthorId = authorId,
                AuthorType = authorType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return new PostResponseDto
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                AuthorType = authorType,
                Content = post.Content,
                CreatedAt = (DateTime)post.CreatedAt
            };
        }

        public async Task<List<PostFeedDto>> GetFeedAsync(int authorId, string authorType)
        {
            var posts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostFeedDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt!.Value,

                    AuthorId = p.AuthorId,
                    AuthorType = p.AuthorType,

                    LikesCount = p.PostLikes.Count,
                    CommentsCount = p.Comments.Count,

                    IsLikedByMe = p.PostLikes.Any(l =>
                        l.AuthorId == authorId &&
                        l.AuthorType == authorType),

                    IsSavedByMe = p.PostSaves.Any(s =>
                        s.AuthorId == authorId &&
                        s.AuthorType == authorType),

                    AuthorName = null!
                })
                .ToListAsync();
            
            var userAuthorIds = posts
                .Where(p => p.AuthorType == "JobSeeker")
                .Select(p => p.AuthorId)
                .Distinct()
                .ToList();

            var companyAuthorIds = posts
                .Where(p => p.AuthorType == "Recruiter")
                .Select(p => p.AuthorId)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => userAuthorIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id
                })
                .ToDictionaryAsync(x => x.Id);

            var companies = await _context.Companies
                .Where(c => companyAuthorIds.Contains(c.Id))
                .Select(c => new
                {
                    c.Id
                })
                .ToDictionaryAsync(x => x.Id);

            return posts;
        }

    }
}
