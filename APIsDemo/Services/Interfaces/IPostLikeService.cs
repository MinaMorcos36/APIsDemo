namespace APIsDemo.Services.Interfaces
{
    public interface IPostLikeService
    {
        Task<bool> ToggleLikeAsync(int postId, int authorId, string authorType);
    }
}
