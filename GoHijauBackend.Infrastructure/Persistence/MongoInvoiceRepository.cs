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
    public class MongoInvoiceRepository : IInvoiceRepository
    {
        private readonly IMongoCollection<Invoice> _invoice;
        private readonly IMongoCollection<InvoiceCounter> _invoiceCounter;

        public MongoInvoiceRepository(IMongoDatabase database)
        {
            _invoice = database.GetCollection<Invoice>("Invoice");
            _invoiceCounter = database.GetCollection<InvoiceCounter>("InvoiceCounter");
        }
        public async Task<Invoice> AddAsync(Invoice invoice)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invoice.OrganizationId))
                    throw new ArgumentException("OrganizationId is required.");

                if (string.IsNullOrWhiteSpace(invoice.MachineId))
                    throw new ArgumentException("MachineId is required.");

                if (invoice.OilCollected <= 0)
                    throw new ArgumentException("OilCollected must be greater than 0.");

                if (invoice.CollectorRate <= 0)
                    throw new ArgumentException("CollectorRate must be greater than 0.");

                invoice.TotalAmount = invoice.OilCollected * invoice.CollectorRate;

                invoice.InvoiceId = await GetNextInvoiceId();

                invoice.CreatedAt = invoice.CreatedAt == default ? DateTime.UtcNow : invoice.CreatedAt;
                invoice.CreatedBy ??= "System";

                await _invoice.InsertOneAsync(invoice);

                return invoice;
            }
            catch (ArgumentException ex)
            {
                throw new Exception("Validation failed for Invoice: " + ex.Message, ex);
            }
            catch (MongoWriteException ex)
            {
                throw new Exception("Database write error while inserting Invoice: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding Invoice: " + ex.Message, ex);
            }
        }

        public async Task<string> GetNextInvoiceId()
        {
            try
            {
                var filter = Builders<InvoiceCounter>.Filter.Eq(x => x.Id, "invoice_sequence");
                var update = Builders<InvoiceCounter>.Update.Inc(x => x.SequenceValue, 1);

                var options = new FindOneAndUpdateOptions<InvoiceCounter>
                {
                    ReturnDocument = ReturnDocument.After,
                    IsUpsert = true
                };

                var counter = await _invoiceCounter.FindOneAndUpdateAsync(filter, update, options);

                return $"INV-{DateTime.UtcNow:yyyy}-{counter.SequenceValue:D6}";
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating InvoiceId sequence: " + ex.Message, ex);
            }
        }
    }
}
