namespace APIsDemo.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string otp);
        string GenerateOtp(int length = 6);
    }
}
