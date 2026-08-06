using CSharpFunctionalExtensions;
using GoHijauBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface ICollectorUCOReminderRepository
    {
        public Task<Reminder> InsertCollectorUCOReminder(Reminder reminder); 
        public Task<Result> UpdateCollectorUCOReminderStatus(string reminderId, string status, Exception? exceptionMessage = null);
        public Task<Reminder> GetReminderByReminderId(string reminderId);
    }
}
