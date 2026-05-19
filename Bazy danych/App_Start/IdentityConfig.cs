using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Bazy_danych
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            var host = ConfigurationManager.AppSettings["MailtrapHost"];
            var port = int.Parse(ConfigurationManager.AppSettings["MailtrapPort"]);
            var user = ConfigurationManager.AppSettings["MailtrapUser"];
            var pass = ConfigurationManager.AppSettings["MailtrapPass"];
            var from = ConfigurationManager.AppSettings["MailtrapFrom"];

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