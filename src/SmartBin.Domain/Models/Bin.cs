using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartBin.Domain.Models
{
    public class Bin : IEntity
    {
        public ObjectId Id { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public BinTelemetry Telemetry { get; set; }
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
