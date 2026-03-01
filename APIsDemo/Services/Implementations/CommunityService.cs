using APIsDemo.DTOs.Community;
using APIsDemo.Models;
using APIsDemo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Services.Implementations
{
    public class CommunityService : ICommunityService
    {
        private readonly AppDbContext _context;

        public CommunityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CommunityFeedItemDto>> GetFeedAsync(int authorId, string authorType, int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var postsQ = _context.Posts
                .Select(p => new CommunityFeedItemDto
                {
                    Id = p.Id,
                    Type = "Post",
                    Title = null,
                    Content = p.Content!,
                    CreatedAt = p.CreatedAt!.Value,
                    AuthorId = p.AuthorId,
                    AuthorType = p.AuthorType!,
                    LikesCount = p.PostLikes.Count,
                    CommentsCount = p.Comments.Count,
                    IsLikedByMe = p.PostLikes.Any(l => l.AuthorId == authorId && l.AuthorType == authorType),
                    IsSavedByMe = p.PostSaves.Any(s => s.AuthorId == authorId && s.AuthorType == authorType),
                    Location = null,
                    AuthorName = string.Empty
                });

            var jobsQ = _context.Jobs
                .Select(j => new CommunityFeedItemDto
                {
                    Id = j.Id,
                    Type = "Job",
                    Title = j.Title,
                    Content = j.Description,
                    CreatedAt = j.CreatedAt!.Value,
                    AuthorId = j.CompanyId,
                    AuthorType = "Recruiter",
                    LikesCount = null,
                    CommentsCount = null,
                    IsLikedByMe = null,
                    IsSavedByMe = null,
                    Location = j.Location,
                    AuthorName = string.Empty
                });

            var combinedQ = postsQ.Concat(jobsQ)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var items = await combinedQ.ToListAsync();

            // Resolve author names
            var userAuthorIds = items
                .Where(i => i.AuthorType == "JobSeeker")
                .Select(i => i.AuthorId)
                .Distinct()
                .ToList();

            var companyAuthorIds = items
                .Where(i => i.AuthorType == "Recruiter")
                .Select(i => i.AuthorId)
                .Distinct()
                .ToList();

            var userNames = await _context.UserProfiles
                .Where(up => userAuthorIds.Contains(up.UserId))
                .Select(up => new { up.UserId, Name = ((up.FirstName ?? "") + " " + (up.LastName ?? "")).Trim() })
                .ToDictionaryAsync(x => x.UserId, x => x.Name);

            var companyNames = await _context.CompanyOverviews
                .Where(co => companyAuthorIds.Contains(co.CompanyId))
                .Select(co => new { co.CompanyId, Name = co.Name ?? string.Empty })
                .ToDictionaryAsync(x => x.CompanyId, x => x.Name);

            foreach (var item in items)
            {
                if (item.AuthorType == "JobSeeker")
                {
                    if (userNames.TryGetValue(item.AuthorId, out var name))
                        item.AuthorName = string.IsNullOrWhiteSpace(name) ? "" : name;
                    else
                        item.AuthorName = "";
                }
                else if (item.AuthorType == "Recruiter")
                {
                    if (companyNames.TryGetValue(item.AuthorId, out var name))
                        item.AuthorName = string.IsNullOrWhiteSpace(name) ? "" : name;
                    else
                        item.AuthorName = "";
                }
                else
                {
                    item.AuthorName = "";
                }
            }

            return items;
        }
    }
}
