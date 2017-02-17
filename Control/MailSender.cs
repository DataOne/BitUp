using DataOne.BitUp.Properties;
using System.Net;
using System.Net.Mail;

namespace DataOne.BitUp.Control
{
    public static class MailSender
    {
        public static void Send(string body, string subject, string pathToLog)
        {
            SmtpClient client = new SmtpClient(Settings.Default.SmtpHost);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(Settings.Default.EmailAccount, Settings.Default.EmailPassword);
            client.Port = Settings.Default.SmtpPort;

            MailMessage mail = new MailMessage(Settings.Default.MailFrom, Settings.Default.SendMailTo);
            mail.Subject = subject ?? "-";
            mail.IsBodyHtml = true;
            mail.Body = body ?? "";

            var attachment = new Attachment(pathToLog);
            mail.Attachments.Add(attachment);

            client.Send(mail);
            attachment.ContentStream.Close();
        }
    }
}
