using GoHijauBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface ICollectorUCOReminder
    {
        public Task SendReminder(Reminder reminder);
        public Task ProcessReminder(string reminderId);
        public Task SendWhatsApp(string phone, string message);
    }
}
