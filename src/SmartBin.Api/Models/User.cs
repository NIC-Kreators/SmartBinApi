using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace SmartBin.Api.Models
{
    public class User
    {
        [BsonId] public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
