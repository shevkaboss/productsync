using ProductSynchronizer.Helpers;
using ProductSynchronizer.Logger;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ProductSynchronizer.Utils
{
    public class EmailSender
    {
        private static readonly string _senderDisplayName = "ProdSyncer";
        public static async Task SendResultLog()
        {
            
            var from = new MailAddress(ConfigHelper.Config.EmailConfig.ResultLogMailSenderMail, _senderDisplayName);
            var to = new MailAddress(ConfigHelper.Config.EmailConfig.ResultLogMailReciever);
            var m = new MailMessage(from, to)
            {
                Subject = $"Result Log {DateTime.Now.ToShortDateString()}",
                Body = "Result log file attached in this mail."
            };
            m.CC.Add(new MailAddress(ConfigHelper.Config.EmailConfig.ResultLogMailCC));

            var resultLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigHelper.ResultLogFilePath);
            m.Attachments.Add(new Attachment(resultLogPath));

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(ConfigHelper.Config.EmailConfig.ResultLogMailSenderMail, ConfigHelper.Config.EmailConfig.ResultLogMailSenderPass),
                EnableSsl = true
            };

            try
            {
                await smtp.SendMailAsync(m);
                m.Dispose();
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message);
            }
        }
    }
}
