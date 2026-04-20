using System.Linq;
using APIsDemo.Models;
using APIsDemo.Models.Enums;

namespace APIsDemo.Services.Extensions
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
    }
}
