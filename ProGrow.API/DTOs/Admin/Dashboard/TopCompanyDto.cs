namespace ProGrow.API.DTOs.Admin.Dashboard
{
    public class TopCompanyDto
    {
        public int CompanyId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int JobsCount { get; set; }

        public int ApplicationsCount { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
