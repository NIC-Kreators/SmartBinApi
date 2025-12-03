using SmartBin.Api.GenericRepository;
using MongoDB.Bson;

namespace SmartBin.Api.Models
{
    public class ShiftLog : IEntity
    {
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public List<ObjectId> CleanedBins { get; set; }
        public double DistanceTravelledKm { get; set; }
        public string Route { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
