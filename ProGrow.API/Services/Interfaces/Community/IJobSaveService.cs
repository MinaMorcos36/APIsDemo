namespace ProGrow.API.Services.Interfaces.Community
{
    public interface IJobSaveService
    {
        Task<bool> ToggleSaveAsync(int jobId);
    }
}
