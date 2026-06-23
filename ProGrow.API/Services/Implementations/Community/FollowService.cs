using ProGrow.API.DTOs.Community.Follows;
using ProGrow.API.Models;
using ProGrow.API.Services.Interfaces.Community;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ProGrow.API.Services.Implementations.Community
{
    public class FollowService : IFollowService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FollowService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetAuthorId()
        {
            return int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        public async Task<bool> ToggleUserFollowAsync(int targetUserId)
        {
            var authorId = GetAuthorId();
            var existing = await _context.UserFollows
                .FirstOrDefaultAsync(ufu => ufu.UserId == authorId && ufu.FollowedUserId == targetUserId);
            if (existing != null)
            {
                _context.UserFollows.Remove(existing);
                await _context.SaveChangesAsync();
                return false;
            }

            if (authorId == targetUserId)
                throw new BadHttpRequestException("Cannot follow yourself.");

            var exists = await _context.Users
                .AnyAsync(u => u.Id == targetUserId);
            if (!exists)
                throw new KeyNotFoundException("Target user not found.");

            _context.UserFollows.Add(new UserFollow
            {
                UserId = authorId,
                FollowedUserId = targetUserId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleCompanyFollowAsync(int targetCompanyId)
        {
            var authorId = GetAuthorId();
            var existing = await _context.UserFollows
                .FirstOrDefaultAsync(ufc => ufc.UserId == authorId && ufc.FollowedCompanyId == targetCompanyId);
            if (existing != null)
            {
                _context.UserFollows.Remove(existing);
                await _context.SaveChangesAsync();
                return false;
            }

            var exists = await _context.Companies.AnyAsync(c => c.Id == targetCompanyId);
            if (!exists)
                throw new KeyNotFoundException("Target company not found.");

            _context.UserFollows.Add(new UserFollow
            {
                UserId = authorId,
                FollowedCompanyId = targetCompanyId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        #region Company follows

        public async Task<bool> ToggleCompanyFollowUserAsync(int targetUserId)
        {
            var companyId = GetAuthorId();
            var existing = await _context.CompanyFollows
                .FirstOrDefaultAsync(cfu => cfu.CompanyId == companyId && cfu.FollowedUserId == targetUserId);
            if (existing != null)
            {
                _context.CompanyFollows.Remove(existing);
                await _context.SaveChangesAsync();
                return false;
            }

            var exists = await _context.Users.AnyAsync(u => u.Id == targetUserId);
            if (!exists)
                throw new KeyNotFoundException("Target user not found.");

            _context.CompanyFollows.Add(new CompanyFollow
            {
                CompanyId = companyId,
                FollowedUserId = targetUserId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleCompanyFollowCompanyAsync(int targetCompanyId)
        {
            var companyId = GetAuthorId();

            var existing = await _context.CompanyFollows
                .FirstOrDefaultAsync(cfc => cfc.CompanyId == companyId && cfc.FollowedCompanyId == targetCompanyId);
            if (existing != null)
            {
                _context.CompanyFollows.Remove(existing);
                await _context.SaveChangesAsync();
                return false;
            }

            if (companyId == targetCompanyId)
                throw new BadHttpRequestException("Cannot follow yourself.");

            var exists = await _context.Companies.AnyAsync(c => c.Id == targetCompanyId);
            if (!exists)
                throw new KeyNotFoundException("Target company not found.");

            _context.CompanyFollows.Add(new CompanyFollow
            {
                CompanyId = companyId,
                FollowedCompanyId = targetCompanyId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Counts

        public async Task<ProfileCountsDto> GetUserProfileCountsAsync(int userId)
        {
            var exists = await _context.Users.AnyAsync(u => u.Id == userId);

            if (!exists)
                throw new KeyNotFoundException("User not found.");

            var followers =
                await _context.UserFollows.CountAsync(x => x.FollowedUserId == userId)
                + await _context.CompanyFollows.CountAsync(x => x.FollowedUserId == userId);

            var followings =
                await _context.UserFollows.CountAsync(x => x.UserId == userId && x.FollowedUserId != null)
                + await _context.UserFollows.CountAsync(x => x.UserId == userId && x.FollowedCompanyId != null);

            return new ProfileCountsDto
            {
                Followers = followers,
                Followings = followings
            };
        }

        public async Task<ProfileCountsDto> GetCompanyOverviewCountsAsync(int companyId)
        {
            var exists = await _context.Companies.AnyAsync(c => c.Id == companyId);

            if (!exists)
                throw new KeyNotFoundException("Company not found.");

            var followers =
                await _context.UserFollows.CountAsync(x => x.FollowedCompanyId == companyId)
                + await _context.CompanyFollows.CountAsync(x => x.FollowedCompanyId == companyId);

            var followings =
                await _context.CompanyFollows.CountAsync(x => x.CompanyId == companyId && x.FollowedUserId != null)
                + await _context.CompanyFollows.CountAsync(x => x.CompanyId == companyId && x.FollowedCompanyId != null);

            return new ProfileCountsDto
            {
                Followers = followers,
                Followings = followings
            };
        }

        #endregion
    }
}
