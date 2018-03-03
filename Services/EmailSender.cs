using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace JwtAuthCore2.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {

        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task SendEmailAsync(string email, string subject, string message)
        {
            try { 
                    SmtpClient client = new SmtpClient(_configuration["emailSender:SmtpClient"]);
                    client.Port = Convert.ToInt32(_configuration["emailSender:SmtpPort"]);
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_configuration["emailSender:UserName"], _configuration["emailSender:Password"]);
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(_configuration["emailSender:MessageFrom"]);
                    mailMessage.To.Add(email);
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Body = message;
                    mailMessage.Subject = subject;
                    client.Send(mailMessage);
            }
            catch(SmtpException sm)
            {
                return Task.FromException(sm);
            }
            return Task.CompletedTask;
        }
    }
}



