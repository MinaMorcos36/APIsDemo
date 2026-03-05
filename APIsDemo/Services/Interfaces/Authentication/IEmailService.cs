namespace APIsDemo.Services.Interfaces.Authentication
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string otp);
        string GenerateOtp(int length = 6);
    }
}
