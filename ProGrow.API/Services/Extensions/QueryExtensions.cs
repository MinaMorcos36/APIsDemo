using System.Linq;
using ProGrow.API.Models;
using ProGrow.API.Models.Enums;

namespace ProGrow.API.Services.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<Job> ApplyJobFilter(this IQueryable<Job> jobs, string? filter)
        {
            return filter?.ToLower() switch
            {
                "active" => jobs.Where(j => j.IsActive == true),
                "closed" => jobs.Where(j => j.IsActive == false),
                _ => jobs
            };
        }

        public static IQueryable<JobApplication> ApplyApplicationFilter(this IQueryable<JobApplication> jobApplications, string? filter)
        {
            // JobApplication.Status is a navigation to JobApplicationStatus entity which stores Name.
            return filter?.ToLower() switch
            {
                "pending" => jobApplications.Where(a => a.Status != null && a.Status.Name == ApplicationStatus.Pending.ToString()),
                "accepted" => jobApplications.Where(a => a.Status != null && a.Status.Name == ApplicationStatus.Accepted.ToString()),
                "rejected" => jobApplications.Where(a => a.Status != null && a.Status.Name == ApplicationStatus.Rejected.ToString()),
                _ => jobApplications
            };
        }

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int? page, int? pageSize)
        {
            if (pageSize == null || pageSize <= 0)
            {
                return query;
            }

            var size = Math.Min(pageSize.Value, 100);
            var currentPage = page.GetValueOrDefault(1);
            if (currentPage <= 0)
            {
                currentPage = 1;
            }

            return query
                .Skip((currentPage - 1) * size)
                .Take(size);
        }
    }
}
