using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoCollectorUCOReminderRepository : ICollectorUCOReminderRepository
    {
        private readonly IMongoCollection<Reminder> _reminders;
        public MongoCollectorUCOReminderRepository(IMongoDatabase db) 
        {
            _reminders = db.GetCollection<Reminder>("Reminders");
        }
        public async Task<Reminder> GetReminderByReminderId(string reminderId)
        {
            var reminder = await _reminders
            .Find(x => x.Id == reminderId)
            .FirstOrDefaultAsync();
            return reminder!=null? reminder: new Reminder();
        }

        public async Task<Reminder> InsertCollectorUCOReminder(Reminder reminder)
        {
             await _reminders.InsertOneAsync(reminder);
             return reminder;
        }

        public async Task<Result> UpdateCollectorUCOReminderStatus(string reminderId, string status, Exception? exceptionMessage = null)
        {
            if (status == "Sent")
            {
                var update = Builders<Reminder>.Update
               .Set(x => x.Status, "Sent");
                var result = await _reminders.UpdateOneAsync(
                  x => x.Id == reminderId,
                  update);

                return Result.Success(result);

            }
            else {
                var update = Builders<Reminder>.Update
                 .Set(x => x.Status, "Failed")
                 .Set(x => x.ErrorMessage, exceptionMessage?.Message);

                var result = await _reminders.UpdateOneAsync(
                    x => x.Id == reminderId,
                    update
                );
                return Result.Success(result);
            }
                
        }
    }
}
