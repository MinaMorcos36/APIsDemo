using APIsDemo.DTOs.Community.Posts;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIsDemo.Services.Implementations.Community
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public async Task<PostResponseDto> CreateAsync(CreatePostDto dto)
        {
            var post = new Post
            {
                Content = dto.Content,
                AuthorId = GetAuthorId(),
                AuthorType = GetAuthorType(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return new PostResponseDto
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                AuthorType = post.AuthorType,
                Content = post.Content,
                CreatedAt = (DateTime)post.CreatedAt
            };
        }

        public async Task<List<PostFeedDto>> GetFeedAsync()
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

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

            var userProfiles = await _context.UserProfiles
                .Where(up => userAuthorIds.Contains(up.UserId))
                .Select(up => new
                {
                    up.UserId,
                    Name = ((up.FirstName ?? string.Empty) + " " + (up.LastName ?? string.Empty)).Trim()
                })
                .ToDictionaryAsync(x => x.UserId);

            var companyOverviews = await _context.CompanyOverviews
                .Where(co => companyAuthorIds.Contains(co.CompanyId))
                .Select(co => new
                {
                    co.CompanyId,
                    Name = co.Name ?? string.Empty
                })
                .ToDictionaryAsync(x => x.CompanyId);

            foreach (var p in posts)
            {
                if (p.AuthorType == "JobSeeker")
                {
                    if (userProfiles.TryGetValue(p.AuthorId, out var up) && !string.IsNullOrWhiteSpace(up.Name))
                        p.AuthorName = up.Name;
                    else
                        p.AuthorName = string.Empty;
                }
                else if (p.AuthorType == "Recruiter")
                {
                    if (companyOverviews.TryGetValue(p.AuthorId, out var co) && !string.IsNullOrWhiteSpace(co.Name))
                        p.AuthorName = co.Name;
                    else
                        p.AuthorName = string.Empty;
                }
                else
                {
                    p.AuthorName = string.Empty;
                }
            }

            return posts;
        }

    }
}
