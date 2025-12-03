using SmartBin.Api.GenericRepository;
using MongoDB.Bson;

namespace SmartBin.Api.Models
{
    public class CleaningLog : IEntity
    {
        public ObjectId Id { get; set; }
        public ObjectId BinId { get; set; }
        public ObjectId UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public int RemovedWeightKg { get; set; }
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
