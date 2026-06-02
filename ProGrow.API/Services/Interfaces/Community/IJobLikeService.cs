namespace ProGrow.API.Services.Interfaces.Community
{
    public interface IJobLikeService
    {
        Task<bool> ToggleLikeAsync(int jobId);
    }
}
