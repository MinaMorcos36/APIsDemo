namespace ProGrow.API.DTOs.Admin.Dashboard
{
    public class DashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }

        public int ApprovedCompanies { get; set; }
        public int PendingCompanies { get; set; }
        public int RejectedCompanies { get; set; }

        public List<StatusCountDto> ApplicationStatuses { get; set; } = [];

        public List<TrendDto> JobsTrend { get; set; } = [];

        public List<TrendDto> ApplicationsTrend { get; set; } = [];

        public List<RecentJobDto> RecentJobs { get; set; } = [];
        public List<TopCompanyDto> TopCompanies { get; set; } = [];
    }
}
