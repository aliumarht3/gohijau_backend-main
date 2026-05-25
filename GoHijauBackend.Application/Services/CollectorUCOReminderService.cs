using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;


namespace GoHijauBackend.Application.Services
{
    public class CollectorUCOReminderService : ICollectorUCOReminder
    {
        private readonly IEmailService _emailService;    
        private readonly IMachineRepository _machineryRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ICollectorUCOReminderRepository _collectorUCOReminderRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CollectorUCOReminderService> _logger;

        public CollectorUCOReminderService(ICollectorUCOReminderRepository collectorUCOReminderRepository, 
            IEmailService emailService, IMachineRepository machineryRepository, IOrganizationRepository organizationRepository,
            IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<CollectorUCOReminderService> logger,
            IUserRepository userRepository)
        {
            _collectorUCOReminderRepository = collectorUCOReminderRepository;
            _emailService = emailService;
            _machineryRepository = machineryRepository;
            _organizationRepository = organizationRepository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _userRepository = userRepository; 
        }


        public async Task SendReminder(Reminder reminder)
        {
            await _collectorUCOReminderRepository.InsertCollectorUCOReminder(reminder);
            BackgroundJob.Schedule(
              () =>
            ProcessReminder(reminder.Id),
            reminder.ReminderTimeUtc
          );
        }

        public async Task ProcessReminder(string reminderId)
        {
            var reminder = await _collectorUCOReminderRepository.GetReminderByReminderId(reminderId);
            if (reminder == null)
                return;

            if (reminder.Status == "Sent")
                return;

            try
            {
                int percentage = 80;
                var machine = await _machineryRepository.GetByMachineIdAsync(reminder.MachineId);
                if (machine == null) return;
                var organization = await _organizationRepository.GetOrganizationById(machine.Collector); 
                if (organization == null) return;
                List<User> collectorUsers = await _userRepository.GetFromOrganizationIdAsync(organization.Id);
                reminder.OrganizationId = organization.Id;
                if (reminder.Message == "100") { percentage = 100; }

                DateTime dateTime = DateTime.Now;

                foreach (var email in organization.NotificationEmails) 
                {
                    await _emailService.BuildAndSendReminderEmail(email, organization.OrganizationName, machine.MachineId, machine.Location.Name, percentage, dateTime);
                }

                foreach (var user in collectorUsers) 
                {
                    if (!string.IsNullOrWhiteSpace(user.Phone))
                    {
                        var whatsAppMessage = BuildWhatsAppReminderMessage(
                            organization.OrganizationName,
                            machine.MachineId,
                            machine.Location.Name,
                            percentage,
                            dateTime
                        );
                        await SendWhatsApp(user.Phone, whatsAppMessage);
                    }

                }
              

                await _collectorUCOReminderRepository.UpdateCollectorUCOReminderStatus(reminderId,"Sent");
            }
            catch (Exception ex)
            {
                await _collectorUCOReminderRepository.UpdateCollectorUCOReminderStatus(reminderId, "Failed", ex);

                throw;
            }
        }

        private string BuildWhatsAppReminderMessage(string organizationName, string machineName, string machineLocation, int capacityPercentage, DateTime sentAtLocal)
        {
            var levelText = capacityPercentage >= 100
                ? "🔴 *Critical – Capacity Full*"
                : "🟡 *Warning – Capacity Reached 80%*";

            var sb = new StringBuilder();
            sb.AppendLine($"Dear *{organizationName}*,");
            sb.AppendLine();
            sb.AppendLine("This is an automated reminder to inform you that one of your machines has reached a critical collection level and requires action.");
            sb.AppendLine();
            sb.AppendLine($"📋 *Reminder Type:* {levelText}");
            sb.AppendLine($"📊 *Capacity Level:* {capacityPercentage}%");
            sb.AppendLine($"🏭 *Machine Name:* {machineName}");
            sb.AppendLine($"📍 *Location:* {machineLocation}");
            sb.AppendLine($"🕐 *Reminder Sent At:* {sentAtLocal:dddd, dd MMM yyyy hh:mm tt}");
            sb.AppendLine();
            sb.AppendLine("Please arrange for oil collection as soon as possible to avoid overflow and operational issues.");
            sb.AppendLine();
            sb.AppendLine("Thank you for your cooperation.");
            sb.AppendLine();
            sb.AppendLine("Best regards,");
            sb.AppendLine("GoHijau Team");

            return sb.ToString();
        }

        public async Task SendWhatsApp(string phone, string message)
        {
            var apiKey = _configuration["OnSend:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OnSend API key is not configured. Add 'OnSend:ApiKey' to appsettings.json.");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                phone_number = phone,
                message = message,
                type = "text",
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            _logger.LogInformation("Sending WhatsApp message to {Phone} via OnSend API", phone);

            var response = await client.PostAsync("https://onsend.io/api/v1/send", jsonContent);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OnSend API failed with status {StatusCode}: {ResponseBody}", response.StatusCode, responseBody);
                throw new HttpRequestException($"OnSend API returned {response.StatusCode}: {responseBody}");
            }

            _logger.LogInformation("WhatsApp message sent successfully to {Phone}. Response: {ResponseBody}", phone, responseBody);
        }

    }
}
