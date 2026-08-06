using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using MimeKit;
using System;
using System.Reflection;
using System.Reflection.PortableExecutable;

namespace GoHijauBackend.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IViewRenderService _viewRenderer;

        public EmailService(IViewRenderService viewRenderer)
        {
            _viewRenderer = viewRenderer;
        }

        public async Task<Result> SendEmail(EmailDto emailDto)
        {
            try
            {
                // Render Razor template with dynamic content
                var htmlBody = await _viewRenderer.RenderToStringAsync("EmailTemplate", emailDto);
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("GoHijau", "admin@loopsphereinn.com"));
                message.To.Add(MailboxAddress.Parse(emailDto.EmailTo));
                message.Subject = emailDto.EmailSubject;

                var builder = new BodyBuilder();

                // Add logo (inline)
                var assembly = Assembly.GetExecutingAssembly();

                // Full resource name → must match `<DefaultNamespace>.<folder>.<file>`
                var resourceName = "GoHijauBackend.Application.wwwroot.images.icon.png";

                byte[] logoBytes;
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new FileNotFoundException($"Resource '{resourceName}' not found. Check Build Action = EmbeddedResource.");

                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    logoBytes = ms.ToArray();
                }
                var logo = builder.LinkedResources.Add("logo.png", logoBytes);
                logo.ContentId = "logoImage";

                // Replace logo placeholder with correct cid
                builder.HtmlBody = htmlBody.Replace("cid:logoImage", $"cid:{logo.ContentId}");
                if (emailDto.EmailAttachmentBytes != null &&
                emailDto.EmailAttachmentBytes.Length > 0 &&
                !string.IsNullOrWhiteSpace(emailDto.EmailAttachmentName))
                            {
                                builder.Attachments.Add(
                                    emailDto.EmailAttachmentName,
                                    emailDto.EmailAttachmentBytes,
                                    new ContentType("application", "pdf")
                                );
                            }
                message.Body = builder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync("mail.smtp2go.com", 2525, false);
                await client.AuthenticateAsync("loopsphereinn.com", "UoWP8SPvfOcAVfZt");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        public async Task<Result> BuildAndSendNewAccountEmail(string recipientEmail, string tempPassword)
        {
            var emailDto = new EmailDto();

            emailDto.EmailContent = BuildNewAccountEmailBody(recipientEmail, tempPassword);
            emailDto.EmailTo = recipientEmail;
            emailDto.EmailSubject = "WELCOME TO GOHIJAU";

            return await SendEmail(emailDto);
        }

        private string BuildNewAccountEmailBody(string email, string tempPassword)
        {
            string url = "https://dashboard.gohijau.org/login";

            return $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Welcome to <span style='color:#4CAF50;'>GoHijau</span>!</h2>
                    <p>We’re thrilled to have you on board. Your account has been successfully created.</p>

                    <h4>Here are your login credentials:</h4>
                    <table style='border-collapse: collapse; margin-top:10px;'>
                        <tr>
                            <td style='padding: 6px 12px; font-weight: bold;'>Email:</td>
                            <td style='padding: 6px 12px; background:#f5f5f5;'>{email}</td>
                        </tr>
                        <tr>
                            <td style='padding: 6px 12px; font-weight: bold;'>Password:</td>
                            <td style='padding: 6px 12px; background:#f5f5f5;'>{tempPassword}</td>
                        </tr>
                    </table>

                    <p style='margin-top:20px;'>
                        You can now log in to your account at {url} and start exploring our platform.
                    </p>

                    <p>
                        Best regards,<br/>
                        <strong>The GoHijau Team</strong><br/>
                        <img src='cid:logoImage' alt='GoHijau Logo' style='height:50px; margin-top:10px;'/>
                    </p>
                </div>
            ";
        }

        public async Task<Result> BuildAndSendInvoiceEmail(string email, string customerName, Invoice invoice, byte[]? pdfBytes)
        {
            var emailDto = new EmailDto();

            emailDto.EmailTo = email;
            emailDto.EmailSubject = $"Invoice {invoice.InvoiceId}";
            emailDto.EmailContent = BuildInvoiceBody(invoice, customerName);
            emailDto.EmailAttachmentBytes = pdfBytes;
            emailDto.EmailAttachmentName = $"{invoice.InvoiceId}.pdf";

            return await SendEmail(emailDto);
        }

        private string BuildInvoiceBody(Invoice invoice, string customerName)
        {
            return $@"
              <div style=""font-family: Arial, sans-serif; background-color:#f7f7f7; margin:0; padding:0;"">
                  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                    <tr>
                      <td align=""center"" style=""padding:20px 0;"">
                        <table width=""600"" cellpadding=""20"" cellspacing=""0"" border=""0"" style=""background:#ffffff; border-radius:6px;"">
                          <tr>
                            <td style=""text-align:left; font-size:18px; font-weight:bold; color:#333;"">
                              Invoice #{invoice.InvoiceId}
                            </td>
                          </tr>
                          <tr>
                            <td style=""font-size:14px; line-height:1.6; color:#444;"">
                              Hi {customerName},<br><br>
                              I hope you're doing well.<br><br>
                              Please find attached the invoice <strong>#{invoice.InvoiceId}</strong> for the recent oil collection provided.
                            </td>
                          </tr>
                          <tr>
                            <td>
                              <table width=""100%"" cellpadding=""10"" cellspacing=""0"" style=""background:#f1f1f1; border-radius:4px; font-size:14px; color:#444;"">
                                <tr>
                                  <td><strong>Invoice Number:</strong></td>
                                  <td>{invoice.InvoiceId}</td>
                                </tr>
                                <tr>
                                  <td><strong>Invoice Date:</strong></td>
                                  <td>{invoice.CreatedAt}</td>
                                </tr>
                                <tr>
                                  <td><strong>Due Date:</strong></td>
                                  <td>{invoice.CreatedAt.AddDays(14)}</td>
                                </tr>
                                <tr>
                                  <td><strong>Total Amount Due:</strong></td>
                                  <td>RM{invoice.TotalAmount}</td>
                                </tr>
                              </table>
                            </td>
                          </tr>

                          <tr>
                            <td style=""font-size:14px; line-height:1.6; color:#444;"">
                              We kindly request that payment be made by the due date above.<br>
                              If you’ve already completed the payment, please disregard this message.<br><br>
                              If you have any questions, feel free to contact us — we’re happy to assist.
                            </td>
                          </tr>

                          <tr>
                            <td style=""font-size:14px; color:#444;"">
                              Thank you for your business and trust in us!
                            </td>
                          </tr>
                          <tr>
                            <td style=""font-size:14px; color:#333;"">
                              Warm regards,<br>
                              <strong>ATIA ROBOTICS SDN BHD</strong><br>
                              <a href=""mailto:billing@atia.com"" style=""color:#0066cc;"">billing@atia.com</a> | 0123705600<br>
                              <a href=""https://www.gohijau.org/"" style=""color:#0066cc;"">GoHijau</a>
                            </td>
                          </tr>

                        </table>
                      </td>
                    </tr>
                  </table>
                </div>
            ";
        }

        public async Task<Result> BuildAndSendReminderEmail(string email, string organizationName, string machineName, string machineLocation, int capacityPercentage, DateTime sentAtLocal)
        {
            EmailDto emailDto = new EmailDto();
            emailDto.EmailSubject = capacityPercentage == 100 ? "Critical – Capacity Full" : "Warning – Capacity Reached 80%";
            emailDto.EmailContent =BuildReminderBody(organizationName, machineName, machineLocation, capacityPercentage, sentAtLocal);
            emailDto.EmailTo = email;
            return await SendEmail(emailDto);
        }

        private string BuildReminderBody(string organizationName,string machineName,string machineLocation,int capacityPercentage,DateTime sentAtLocal)
        {
                        var levelText = capacityPercentage >= 100
                            ? "Critical – Capacity Full"
                            : "Warning – Capacity Reached 80%";

                        var html = $@"
            <div style='font-family: Arial, sans-serif; font-size:14px; color:#333; line-height:1.6;'>
                <p>Dear {organizationName},</p>

                <p>
                    This is an automated reminder to inform you that one of your machines
                    has reached a critical collection level and requires action.
                </p>

                <div style='background:#f9f9f9; padding:15px; border-left:4px solid #d32f2f; margin:15px 0;'>
                    <p style='margin:0;'><strong>Reminder Type:</strong> {levelText}</p>
                    <p style='margin:5px 0 0 0;'>
                        <strong>Capacity Level:</strong> {capacityPercentage}%
                    </p>
                    <p style='margin:5px 0 0 0;'>
                        <strong>Machine Name:</strong> {machineName}
                    </p>
                    <p style='margin:5px 0 0 0;'>
                        <strong>Location:</strong> {machineLocation}
                    </p>
                    <p style='margin:10px 0 0 0; color:#555;'>
                        <strong>Reminder Sent At:</strong> {sentAtLocal:dddd, dd MMM yyyy hh:mm tt}
                    </p>
                </div>

                <p>
                    Please arrange for oil collection as soon as possible to avoid overflow
                    and operational issues.
                </p>

                <p>
                    Thank you for your cooperation.
                </p>

                <p>
                    Best regards,<br/>
                    GoHijau Team
                </p>
            </div>";

            return html;
        }

        public async Task<Result> BuildAndSendPassswordResetEmail(string recipientEmail, string primaryLink, string? deepLink = null)
        {
            var emailDto = BuildPasswordResetEmailDto(recipientEmail, primaryLink, deepLink);

            return await SendEmail(emailDto);
        }

        private EmailDto BuildPasswordResetEmailDto(string recipientEmail, string primaryLink, string? deepLink = null)
        {
            var deepLinkSection = string.Empty;
            if (!string.IsNullOrWhiteSpace(deepLink))
            {
                deepLinkSection = $@"
                    <p style='margin-top:12px;'>
                        <strong>Mobile users:</strong> If you have the GoHijau app installed, tap the following link to open the app:<br/>
                        <a href='{deepLink}' style='color:#4CAF50;text-decoration:underline;'>{deepLink}</a><br/>
                        (If the link above is not clickable on your mail client, copy and paste it into your device's browser or use the button below.)
                    </p>";
            }

            return new EmailDto
            {
                EmailTo = recipientEmail,
                EmailSubject = "GoHijau Password Reset",
                EmailContent = $@"
                    <div style='font-family: Arial, sans-serif; color: #333;'>
                        <h3>Password Reset Request</h3>
                        <p>If you requested a password reset, click the button below to set a new password. This link expires in 1 hour.</p>
                        <p><a href='{primaryLink}' style='display:inline-block;padding:10px 16px;background:#4CAF50;color:#fff;text-decoration:none;border-radius:4px;'>Reset Password</a></p>
                        {deepLinkSection}
                        <p>If you did not request this, safely ignore this email.</p>
                        <p>Best,<br/>GoHijau Team</p>
                    </div>"
            };
        }
    }
}
