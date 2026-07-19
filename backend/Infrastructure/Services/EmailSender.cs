using System.Net;
using System.Net.Mail;
using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var fromPass = _configuration.GetValue<string>("EmailSettings:Password");
            var fromEmail = _configuration.GetValue<string>("EmailSettings:FromEmail");
            var fromName = _configuration.GetValue<string>("EmailSettings:FromName");

            if (string.IsNullOrWhiteSpace(fromPass) || string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new BusinessRuleException("Email settings are not configured correctly.");
            }

            MailMessage message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                To = { new MailAddress(to) },
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            var smtpClient = new SmtpClient(_configuration.GetValue<string>("EmailSettings:Host"))
            {
                Port = _configuration.GetValue<int>("EmailSettings:Port"),
                Credentials = new NetworkCredential(fromEmail, fromPass),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}