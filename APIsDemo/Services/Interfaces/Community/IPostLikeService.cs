namespace APIsDemo.Services.Interfaces.Community
{
    public interface IPostLikeService
    {
        Task<bool> ToggleLikeAsync(int postId);
    }
}
