using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Bazy_danych
{
    public class EmailService : IIdentityMessageService
    {
        private static string GetRequiredAppSetting(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException("Missing or empty app setting: " + key);
            }

            return value;
        }

        private static int GetRequiredAppSettingInt(string key)
        {
            var value = GetRequiredAppSetting(key);
            if (!int.TryParse(value, out var parsedValue))
            {
                throw new ConfigurationErrorsException("Invalid integer app setting: " + key);
            }

            return parsedValue;
        }

        public Task SendAsync(IdentityMessage message)
        {
            var host = GetRequiredAppSetting("MailtrapHost");
            var port = GetRequiredAppSettingInt("MailtrapPort");
            var user = GetRequiredAppSetting("MailtrapUser");
            var pass = GetRequiredAppSetting("MailtrapPass");
            var from = GetRequiredAppSetting("MailtrapFrom");

            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true
            };

            var mail = new MailMessage(from, message.Destination)
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true
            };

            return client.SendMailAsync(mail);
        }
    }
}