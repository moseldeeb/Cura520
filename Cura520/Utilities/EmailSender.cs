using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential("Curamvc@gmail.com", "nfxl rbcy ithe gvrk")
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("Curamvc@gmail.com", "Cura Clinic Management"), 
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        return client.SendMailAsync(mailMessage);
    }
}