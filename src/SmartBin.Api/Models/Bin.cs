using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartBin.Api.Models
{
    public class Bin
    {
        [BsonId] public ObjectId Id { get; set; }
        public string Location { get; set; }
        public int FillLevel { get; set; }
    }
}
