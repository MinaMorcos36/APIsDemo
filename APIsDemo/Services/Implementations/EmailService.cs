using MimeKit;
using MailKit.Net.Smtp;
using APIsDemo.Services.Interfaces;

namespace APIsDemo.Services.Implementations
{
    public class EmailService : IEmailService
    {
        public string GenerateOtp(int length = 6)
        {
            var random = new Random();
            string otp = "";
            for (int i = 0; i < length; i++)
                otp += random.Next(0, 10);
            return otp;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("APIsDemo", "minanew0@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = "Verify your email";
            emailMessage.Body = new TextPart("plain") { Text = $"Your OTP code is: {otp}" };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, false); // Gmail SMTP
            await client.AuthenticateAsync("minanew0@gmail.com", "ntchjswahrjqaqer"); // use App Password
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }

    }

}
