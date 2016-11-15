using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;

namespace Portal.Helpers
{
    public class SendMailHelper
    {
        public static string MailUsername { get; set; }
        public static string MailPassword { get; set; }
        public static string MailHost { get; set; }
        public static int MailPort { get; set; }
        public static bool MailSSL { get; set; }

        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }

        static SendMailHelper()
        {
            MailHost = "smtp.gmail.com";
            MailPort = 587; // Mail can use ports 25, 465 & 587; but must be 25 for medium trust environment.
            MailSSL = true;
        }

        public void Send()
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Host = MailHost;
            smtp.Port = MailPort;
            smtp.EnableSsl = MailSSL;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(MailUsername, MailPassword);

            using (var message = new MailMessage(MailUsername, ToEmail))
            {
                message.Subject = Subject;
                message.Body = Body;
                message.IsBodyHtml = IsHtml;
                smtp.Send(message);
            }
        }
    }
}