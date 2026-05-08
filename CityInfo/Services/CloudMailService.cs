namespace CityInfo.Services
{
    public class CloudMailService : IMailService
    {
        private string _mailTo = "admin@pretzbuster.com";
        private string _mailFrom = "noreply@pretzbuster.com";

        public CloudMailService(IConfiguration configuration)
        {
            _mailTo = configuration["mailSettings:mailTo"] ?? _mailTo;
            _mailFrom = configuration["mailSettings:mailFrom"] ?? _mailFrom;
        }

        public void Send(string subject, string message)
        {
            Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, with {nameof(CloudMailService)}.");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {message}");
        }
    }
}
