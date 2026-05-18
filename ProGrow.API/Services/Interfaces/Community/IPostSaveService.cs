namespace ProGrow.API.Services.Interfaces.Community
{
    public interface IPostSaveService
    {
        Task<bool> ToggleSaveAsync(int postId);
    }
}
