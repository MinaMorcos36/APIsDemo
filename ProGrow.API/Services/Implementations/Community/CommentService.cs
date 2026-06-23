using ProGrow.API.DTOs.Community.Comments;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ProGrow.API.Services.Implementations.Community
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommentService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public async Task<CommentResponseDto> CreateAsync(int postId, CreateCommentDto dto)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                throw new Exception("Post not found");

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
                AuthorId = GetAuthorId(),
                AuthorType = GetAuthorType(),
                ParentCommentId = dto.ParentCommentId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return new CommentResponseDto
            {
                Id = comment.Id,
                AuthorId = comment.AuthorId,
                AuthorType = comment.AuthorType,
                PostId = postId,
                JobId = comment.JobId,
                ParentCommentId = comment.ParentCommentId,
                Content = comment.Content,
                AuthorName = comment.AuthorType == "JobSeeker"
                    ? _context.UserProfiles
                        .Where(up => up.UserId == comment.AuthorId)
                        .Select(up => ((up.FirstName ?? string.Empty) + " " + (up.LastName ?? string.Empty)).Trim())
                        .FirstOrDefault() ?? string.Empty
                    : comment.AuthorType == "Recruiter"
                        ? _context.CompanyOverviews
                            .Where(co => co.CompanyId == comment.AuthorId)
                            .Select(co => co.Name)
                            .FirstOrDefault() ?? string.Empty
                        : string.Empty,

                AuthorPictureUrl = comment.AuthorType == "JobSeeker"
                    ? _context.UserProfiles
                        .Where(up => up.UserId == comment.AuthorId)
                        .Select(up => up.PictureUrl)
                        .FirstOrDefault()
                    : comment.AuthorType == "Recruiter"
                        ? _context.CompanyOverviews
                            .Where(co => co.CompanyId == comment.AuthorId)
                            .Select(co => co.PictureUrl)
                            .FirstOrDefault()
                        : null,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<IEnumerable<CommentDto>> GetByPostIdAsync(int postId)
        {
            var comments = await _context.Comments
                .AsNoTracking()
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
                AuthorName = c.AuthorType == "JobSeeker"
                    ? _context.UserProfiles
                        .Where(up => up.UserId == c.AuthorId)
                        .Select(up => ((up.FirstName ?? string.Empty) + " " + (up.LastName ?? string.Empty)).Trim())
                        .FirstOrDefault() ?? string.Empty
                    : c.AuthorType == "Recruiter"
                        ? _context.CompanyOverviews
                            .Where(co => co.CompanyId == c.AuthorId)
                            .Select(co => co.Name)
                            .FirstOrDefault() ?? string.Empty
                        : string.Empty,

                AuthorPictureUrl = c.AuthorType == "JobSeeker"
                    ? _context.UserProfiles
                        .Where(up => up.UserId == c.AuthorId)
                        .Select(up => up.PictureUrl)
                        .FirstOrDefault()
                    : c.AuthorType == "Recruiter"
                        ? _context.CompanyOverviews
                            .Where(co => co.CompanyId == c.AuthorId)
                            .Select(co => co.PictureUrl)
                            .FirstOrDefault()
                        : null,
                CreatedAt = c.CreatedAt,
                Replies = c.InverseParentComment.Select(r => new CommentDto
                {
                    Id = r.Id,
                    Content = r.Content,
                    AuthorId = r.AuthorId,
                    AuthorType = r.AuthorType,
                    AuthorName = r.AuthorType == "JobSeeker"
                        ? _context.UserProfiles
                            .Where(up => up.UserId == r.AuthorId)
                            .Select(up => ((up.FirstName ?? string.Empty) + " " + (up.LastName ?? string.Empty)).Trim())
                            .FirstOrDefault() ?? string.Empty
                        : r.AuthorType == "Recruiter"
                            ? _context.CompanyOverviews
                                .Where(co => co.CompanyId == r.AuthorId)
                                .Select(co => co.Name)
                                .FirstOrDefault() ?? string.Empty
                            : string.Empty,

                    AuthorPictureUrl = r.AuthorType == "JobSeeker"
                        ? _context.UserProfiles
                            .Where(up => up.UserId == r.AuthorId)
                            .Select(up => up.PictureUrl)
                            .FirstOrDefault()
                        : r.AuthorType == "Recruiter"
                            ? _context.CompanyOverviews
                                .Where(co => co.CompanyId == r.AuthorId)
                                .Select(co => co.PictureUrl)
                                .FirstOrDefault()
                            : null,
                    CreatedAt = r.CreatedAt
                }).ToList()
            });
        }

    public async Task<CommentResponseDto> CreateForJobAsync(int jobId, CreateCommentDto dto)
    {
        var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);
        if (!jobExists)
            throw new Exception("Job not found");

        if (dto.ParentCommentId != null)
        {
            var parentExists = await _context.Comments
                .AnyAsync(c => c.Id == dto.ParentCommentId && c.JobId == jobId);

            if (!parentExists)
                throw new Exception("Invalid parent comment");
        }

        var comment = new Comment
        {
            JobId = jobId,
            AuthorId = GetAuthorId(),
            AuthorType = GetAuthorType(),
            ParentCommentId = dto.ParentCommentId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return new CommentResponseDto
        {
            Id = comment.Id,
            AuthorId = comment.AuthorId,
            AuthorType = comment.AuthorType,
            PostId = comment.PostId,
            JobId = jobId,
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            AuthorName = comment.AuthorType == "JobSeeker"
                    ? _context.UserProfiles
                        .Where(up => up.UserId == comment.AuthorId)
                        .Select(up => ((up.FirstName ?? string.Empty) + " " + (up.LastName ?? string.Empty)).Trim())
                        .FirstOrDefault() ?? string.Empty
                    : comment.AuthorType == "Recruiter"
                        ? _context.CompanyOverviews
                            .Where(co => co.CompanyId == comment.AuthorId)
                            .Select(co => co.Name)
                            .FirstOrDefault() ?? string.Empty
                        : string.Empty,

            AuthorPictureUrl = comment.AuthorType == "JobSeeker"
                    ? _context.UserProfiles
                        .Where(up => up.UserId == comment.AuthorId)
                        .Select(up => up.PictureUrl)
                        .FirstOrDefault()
                    : comment.AuthorType == "Recruiter"
                        ? _context.CompanyOverviews
                            .Where(co => co.CompanyId == comment.AuthorId)
                            .Select(co => co.PictureUrl)
                            .FirstOrDefault()
                        : null,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<IEnumerable<CommentDto>> GetByJobIdAsync(int jobId)
    {
        var comments = await _context.Comments
            .AsNoTracking()
            .Where(c => c.JobId == jobId && c.ParentCommentId == null)
            .Include(c => c.InverseParentComment)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            AuthorId = c.AuthorId,
            AuthorType = c.AuthorType,
            AuthorName = c.AuthorType == "JobSeeker"
                    ? _context.UserProfiles
                        .Where(up => up.UserId == c.AuthorId)
                        .Select(up => ((up.FirstName ?? string.Empty) + " " + (up.LastName ?? string.Empty)).Trim())
                        .FirstOrDefault() ?? string.Empty
                    : c.AuthorType == "Recruiter"
                        ? _context.CompanyOverviews
                            .Where(co => co.CompanyId == c.AuthorId)
                            .Select(co => co.Name)
                            .FirstOrDefault() ?? string.Empty
                        : string.Empty,

            AuthorPictureUrl = c.AuthorType == "JobSeeker"
                    ? _context.UserProfiles
                        .Where(up => up.UserId == c.AuthorId)
                        .Select(up => up.PictureUrl)
                        .FirstOrDefault()
                    : c.AuthorType == "Recruiter"
                        ? _context.CompanyOverviews
                            .Where(co => co.CompanyId == c.AuthorId)
                            .Select(co => co.PictureUrl)
                            .FirstOrDefault()
                        : null,
            CreatedAt = c.CreatedAt,
            Replies = c.InverseParentComment.Select(r => new CommentDto
            {
                Id = r.Id,
                Content = r.Content,
                AuthorId = r.AuthorId,
                AuthorType = r.AuthorType,
                AuthorName = c.AuthorType == "JobSeeker"
                    ? _context.UserProfiles
                        .Where(up => up.UserId == c.AuthorId)
                        .Select(up => ((up.FirstName ?? string.Empty) + " " + (up.LastName ?? string.Empty)).Trim())
                        .FirstOrDefault() ?? string.Empty
                    : c.AuthorType == "Recruiter"
                        ? _context.CompanyOverviews
                            .Where(co => co.CompanyId == c.AuthorId)
                            .Select(co => co.Name)
                            .FirstOrDefault() ?? string.Empty
                        : string.Empty,

                AuthorPictureUrl = c.AuthorType == "JobSeeker"
                    ? _context.UserProfiles
                        .Where(up => up.UserId == c.AuthorId)
                        .Select(up => up.PictureUrl)
                        .FirstOrDefault()
                    : c.AuthorType == "Recruiter"
                        ? _context.CompanyOverviews
                            .Where(co => co.CompanyId == c.AuthorId)
                            .Select(co => co.PictureUrl)
                            .FirstOrDefault()
                        : null,
                CreatedAt = r.CreatedAt
            }).ToList()
        });
    }
    }
}
