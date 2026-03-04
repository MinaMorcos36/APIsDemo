namespace APIsDemo.Services.Interfaces
{
    public interface IPostSaveService
    {
        Task<bool> ToggleSaveAsync(int postId);
    }
}
