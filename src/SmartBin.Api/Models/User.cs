using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SmartBin.Api.GenericRepository;
namespace SmartBin.Api.Models
{
    public class User : IEntity
    {
        public ObjectId Id { get; set; }
        public string Role { get; set; }
        public string Nickname { get; set; }
        public string FullName { get; set; }
        public string PasswordHash { get; set; }

        public bool PasswordRecreationRequired { get; set; } = false;
        public DateTime PasswordLastChangedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
