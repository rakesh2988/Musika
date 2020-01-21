using Musika.Library.Validations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Musika.Library.Utilities;

namespace Musika.Library.Utilities
{
    public static class EmailHelper
    {
        public static async Task<string> SendEmail(string To, string Subject, string BodyText)
        {
            return await Task.Factory.StartNew(() =>
                 {
                     if (FValidations.IsValidEmail(To))
                     {
                         MailMessage mailMessage = new MailMessage(ConfigurationManager.AppSettings["ADMIN_EMAIL"].ToString(), To, Subject, BodyText);
                         SmtpClient smtp = new SmtpClient();
                         try
                         {
                             mailMessage.IsBodyHtml = true;
                             smtp.Send(mailMessage);

                             return "Success";
                         }
                         catch (Exception ex)
                         {
                             return "Error";
                         }
                         finally
                         {
                             mailMessage.Dispose();
                             smtp = null;
                         }
                     }
                     else
                     {
                         return "Invalid EmailAddress";
                     }
                 });
        }
    }

    public static class SendEmailHelper
    {
        public static async Task<string> SendMailByThirdParty(string To, string Subject, string BodyText)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.sendgrid.net");

            string mailFrom = System.Configuration.ConfigurationManager.AppSettings["ADMIN_EMAIL"].ToString();
            mail.From = new MailAddress(mailFrom);
            mail.To.Add(To);
            mail.Subject = "New Staff Member Registration";

            mail.Body = BodyText;

            SmtpServer.Port = 587;      // 25;
            SmtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["ApiKey"], ConfigurationManager.AppSettings["ApiKeyPass"]);
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
            return "Success";
        }

        #region "Send Mail for Ticketing App"
        public static bool SendMail(string To, string Subject, string BodyText, string filepath)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient();

                mail.From = new MailAddress("boletas@musikaapp.com");
                mail.To.Add(To);
                mail.Subject = Subject;
                mail.Body = BodyText;
                mail.IsBodyHtml = true;
                System.Net.Mail.Attachment attachment;
                //string filePath = Server.MapPath("/Content/QRCodeImages/" + img);
                if (!string.IsNullOrEmpty(filepath))
                {
                    attachment = new System.Net.Mail.Attachment(filepath);
                    mail.Attachments.Add(attachment);
                }
                SmtpServer.Host = "smtp.gmail.com";
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("boletas@musikaapp.com", "Tickets@a123");
                SmtpServer.EnableSsl = true;


                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool SendMailWithAttachment(string To, string Subject, string BodyText, List<string> filepath, Attachment pdf)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient();

                mail.From = new MailAddress("boletas@musikaapp.com");
                mail.To.Add(To);
                mail.Subject = Subject;
                mail.Body = BodyText;
                mail.IsBodyHtml = true;
                System.Net.Mail.Attachment Attachment;
                //string filePath = Server.MapPath("/Content/QRCodeImages/" + img);
                if (filepath.Count > 0)
                {
                    foreach (string file in filepath)
                    {
                        Attachment = new System.Net.Mail.Attachment(file);
                        mail.Attachments.Add(Attachment);
                    }
                }
                mail.Attachments.Add(pdf);
                SmtpServer.Host = "smtp.gmail.com";
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("boletas@musikaapp.com", "Tickets@a123");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
    }
}
