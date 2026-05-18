using ProGrow.API.DTOs.Community.Posts;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Community;
using ProGrow.API.Services.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ProGrow.API.Services.Implementations.Community
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

        public async Task<List<PostFeedDto>> GetFeedAsync(int? page = null, int? pageSize = null)
        {
            var authorId = GetAuthorId();
            var authorType = GetAuthorType();

            var query = _context.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ApplyPaging(page, pageSize)
                .Select(p => new PostFeedDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt!.Value,

                    AuthorId = p.AuthorId,
                    AuthorType = p.AuthorType!,

                    LikesCount = p.PostLikes.Count,
                    CommentsCount = p.Comments.Count,

                    IsLikedByMe = p.PostLikes.Any(l =>
                        l.AuthorId == authorId &&
                        l.AuthorType == authorType),

                    IsSavedByMe = p.PostSaves.Any(s =>
                        s.AuthorId == authorId &&
                        s.AuthorType == authorType),

                    AuthorName = p.AuthorType == "JobSeeker"
                        ? _context.UserProfiles
                            .Where(up => up.UserId == p.AuthorId)
                            .Select(up => ((up.FirstName ?? string.Empty) + " " + (up.LastName ?? string.Empty)).Trim())
                            .FirstOrDefault() ?? string.Empty
                        : p.AuthorType == "Recruiter"
                            ? _context.CompanyOverviews
                                .Where(co => co.CompanyId == p.AuthorId)
                                .Select(co => co.Name)
                                .FirstOrDefault() ?? string.Empty
                            : string.Empty
                });

            return await query.ToListAsync();
        }

    }
}
