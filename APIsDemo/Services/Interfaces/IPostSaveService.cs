namespace APIsDemo.Services.Interfaces
{
    public interface IPostSaveService
    {
        Task<bool> ToggleSaveAsync(int postId, int authorId, string authorType);
    }
}
