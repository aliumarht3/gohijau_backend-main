using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoOrganizationRepository : IOrganizationRepository
    {
        private readonly IMongoCollection<Organization> _collection;

        public MongoOrganizationRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Organization>("Organizations");
        }
        public async Task AddAsync(Organization organization) =>
            await _collection.InsertOneAsync(organization);

        public async Task<List<Organization>> GetAllOrganizationsAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Organization>> GetOrganizationsByIds(List<string> organizationIds)
        {
            return await _collection
                .Find(o => organizationIds.Contains(o.Id))
                .ToListAsync();
        }

        public async Task UpdateMachineOwnerRate(double machineOwnerRate, string organizationId, string userId)
        {
            var update = Builders<Organization>.Update
                        .Set(x => x.ProfitRate, machineOwnerRate)
                        .Set(x => x.ModifiedBy, userId)
                        .Set(x => x.ModifiedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(
                Builders<Organization>.Filter.Eq(x => x.Id, organizationId),
                update,
                new UpdateOptions { IsUpsert = false }
            );
        }

        public async Task UpdateCollectorRate(double collectorRate, string organizationId, string userId)
        {
            var update = Builders<Organization>.Update
                        .Set(x => x.CollectorRate, collectorRate)
                        .Set(x => x.ModifiedBy, userId)
                        .Set(x => x.ModifiedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(
                Builders<Organization>.Filter.Eq(x => x.Id, organizationId),
                update,
                new UpdateOptions { IsUpsert = false }
            );
        }

        public async Task AddOrganizationEmails(IEnumerable<string> invoiceEmails, IEnumerable<string> notificationEmails, string organizationId, string userId)
        {
            var updates = new List<UpdateDefinition<Organization>>();

            if (invoiceEmails.Any())
                updates.Add(Builders<Organization>.Update.AddToSetEach(o => o.InvoiceEmails, invoiceEmails));

            if (notificationEmails.Any())
                updates.Add(Builders<Organization>.Update.AddToSetEach(o => o.NotificationEmails, notificationEmails));

            if (!updates.Any())
                return;

            updates.Add(Builders<Organization>.Update.Set(o => o.ModifiedBy, userId));
            updates.Add(Builders<Organization>.Update.Set(o => o.ModifiedAt, DateTime.UtcNow));

            var combined = Builders<Organization>.Update.Combine(updates);

            await _collection.UpdateOneAsync(
                Builders<Organization>.Filter.Eq(o => o.Id, organizationId),
                combined,
                new UpdateOptions { IsUpsert = false }
            );
        }

        public async Task<Organization> GetOrganizationById(string organizationId)
        {
           return await _collection.Find(org=> org.Id == organizationId).FirstOrDefaultAsync();
        }

        public async Task UpdateOrganizationAsync(string organizationId, Organization organization, IClientSessionHandle? session = null)
        {
            var update = Builders<Organization>.Update
                .Set(x => x.OrganizationName, organization.OrganizationName)
                .Set(x => x.Address, organization.Address)
                .Set(x => x.OrganizationTypes, organization.OrganizationTypes)
                .Set(x => x.CertificatePath, organization.CertificatePath)
                .Set(x => x.CollectorRate, organization.CollectorRate)
                .Set(x => x.CustomerRate, organization.CustomerRate)
                .Set(x => x.ProfitRate, organization.ProfitRate)
                .Set(x => x.CreditLimit, organization.CreditLimit)
                .Set(x => x.InvoiceEmails, organization.InvoiceEmails)
                .Set(x => x.NotificationEmails, organization.NotificationEmails)
                .Set(x => x.ModifiedBy, organization.ModifiedBy)
                .Set(x => x.ModifiedAt, organization.ModifiedAt);

            var filter = Builders<Organization>.Filter.Eq(x => x.Id, organizationId);
            if (session != null)
            {
                await _collection.UpdateOneAsync(
                    session,
                    filter,
                    update,
                    new UpdateOptions { IsUpsert = false }
                );
            }
            else
            {
                await _collection.UpdateOneAsync(
                    filter,
                    update,
                    new UpdateOptions { IsUpsert = false }
                );
            }
        }

        public async Task<bool> AddOrganizationTotalNOutstandingDebt
        (
            string organizationId,
            double debt,
            string modifiedBy,
            DateTime modifiedAt,
            IClientSessionHandle? session = null
        )
        {
            var filter = Builders<Organization>.Filter.Eq(x => x.Id, organizationId);

            var update = Builders<Organization>.Update
                .Inc(x => x.TotalDebtAssigned, debt)
                .Inc(x => x.OutstandingDebt, debt)
                .Set(x => x.ModifiedBy, modifiedBy)
                .Set(x => x.ModifiedAt, modifiedAt);

            UpdateResult result;

            if (session != null)
            {
                result = await _collection.UpdateOneAsync(
                    session,
                    filter,
                    update
                );
            }
            else
            {
                result = await _collection.UpdateOneAsync(
                    filter,
                    update
                );
            }

            return result.MatchedCount > 0;
        }

        public async Task<bool> UpdateOrganizationOutstandingDebt
        (
            string organizationId,
            double debt,
            string modifiedBy,
            DateTime modifiedAt,
            IClientSessionHandle? session = null
        )
        {
            var filter = Builders<Organization>.Filter.Eq(x => x.Id, organizationId);

            var update = Builders<Organization>.Update
                .Set(x => x.OutstandingDebt, debt)
                .Set(x => x.ModifiedBy, modifiedBy)
                .Set(x => x.ModifiedAt, modifiedAt);

            UpdateResult result;

            if (session != null)
            {
                result = await _collection.UpdateOneAsync(
                    session,
                    filter,
                    update
                );
            }
            else
            {
                result = await _collection.UpdateOneAsync(
                    filter,
                    update
                );
            }

            return result.MatchedCount > 0;
        }

        public async Task DeleteOrganizationAsync(string organizationId)
        {
            await _collection.DeleteOneAsync(
                Builders<Organization>.Filter.Eq(x => x.Id, organizationId)
            );
        }
    }
}
