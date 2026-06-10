using ProGrow.API.DTOs.Community.Posts;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Community;
using ProGrow.API.Services.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ProGrow.API.Services.Implementations.Community
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg"
        };
        private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;

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

        private static string? BuildPhotoPath(string pictureUrl)
        {
            var normalized = pictureUrl.Trim();
            if (!normalized.StartsWith("/", StringComparison.Ordinal))
            {
                normalized = "/" + normalized;
            }

            if (!normalized.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            normalized = normalized.TrimStart('/');
            normalized = normalized.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", normalized);
        }

        private static string? SaveMediaFile(IFormFile mediaFile)
        {
            if (mediaFile == null || mediaFile.Length == 0)
                return null;

            if (mediaFile.Length > MaxPhotoSizeBytes)
                throw new InvalidOperationException("Max file size is 5 MB.");

            var mediaExtension = Path.GetExtension(mediaFile.FileName);
            if (string.IsNullOrWhiteSpace(mediaExtension) || !AllowedImageExtensions.Contains(mediaExtension))
                throw new InvalidOperationException("Invalid file type. Allowed: png, jpg, jpeg.");

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos", "posts");
            Directory.CreateDirectory(uploadsRoot);

            var safeFileName = $"{Guid.NewGuid():N}{mediaExtension.ToLowerInvariant()}";
            var savedPath = Path.Combine(uploadsRoot, safeFileName);
            using (var stream = new FileStream(savedPath, FileMode.Create))
            {
                mediaFile.CopyTo(stream);
            }

            return $"/uploads/photos/posts/{safeFileName}";
        }

        public async Task<PostResponseDto> CreateAsync(CreatePostDto dto, IFormFile? mediaFile)
        {
            string? mediaUrl = null;

            // Save media file if provided
            if (mediaFile != null && mediaFile.Length > 0)
            {
                mediaUrl = SaveMediaFile(mediaFile);
            }

            var post = new Post
            {
                Content = dto.Content,
                AuthorId = GetAuthorId(),
                AuthorType = GetAuthorType(),
                PostMediaUrl = mediaUrl,
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
                PostMediaUrl = post.PostMediaUrl,
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
                    PostMediaUrl = p.PostMediaUrl,
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
                            : string.Empty,

                    AuthorPictureUrl = p.AuthorType == "JobSeeker"
                        ? _context.UserProfiles
                            .Where(up => up.UserId == p.AuthorId)
                            .Select(up => up.PictureUrl)
                            .FirstOrDefault()
                        : p.AuthorType == "Recruiter"
                            ? _context.CompanyOverviews
                                .Where(co => co.CompanyId == p.AuthorId)
                                .Select(co => co.PictureUrl)
                                .FirstOrDefault()
                            : null,

                    AuthorSubtitle = p.AuthorType == "JobSeeker"
                        ? _context.UserProfiles
                            .Where(up => up.UserId == p.AuthorId)
                            .Select(up => up.Headline)
                            .FirstOrDefault()

                            : p.AuthorType == "Recruiter"
                            ? _context.CompanyOverviews
                                .Where(co => co.CompanyId == p.AuthorId)
                                .Select(co => co.Industry.Name)
                                .FirstOrDefault()

                            : null,
                });

            return await query.ToListAsync();
        }

    }
}
