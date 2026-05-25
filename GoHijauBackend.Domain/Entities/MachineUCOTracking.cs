using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Domain.Entities
{
    public class MachineUCOTracking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public  string Id { get; set; }
        [BsonElement("MachineId")]
        public  string MachineId { get; set; }
        [BsonElement("MachineLocationName")]
        public string MachineLocationName { get; set; } = ""; 
        [BsonElement("BufferVolume")]
        public  double BufferVolume { get; set; } = 0;
        [BsonElement("CreatedAt")]
        public  DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [BsonElement("ModifiedAt")]
        public DateTime ModifiedAt { get; set; }

    }
}
