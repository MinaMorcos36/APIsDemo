namespace ProGrow.API.DTOs.Community.Follows
{
    public class FollowedAccountsDto
    {
        public int FollowedUserId { get; set; }
        public int FollowerCompanyId { get; set; }
        public int AuthorId { get; set; }
        public DateTime? FollowedAt { get; set; }
    }
}
