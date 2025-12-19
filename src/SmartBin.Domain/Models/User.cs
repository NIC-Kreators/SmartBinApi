using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartBin.Domain.Models
{
    public class User : IEntity
    {
        public ObjectId Id { get; set; }
        public UserRole Role { get; set; } = GuestRole.Instance;
        public string Nickname { get; set; }
        public string FullName { get; set; }
        public string PasswordHash { get; set; }

        public bool PasswordRecreationRequired { get; set; } = false;
        public DateTime PasswordLastChangedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
