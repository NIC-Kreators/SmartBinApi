using MongoDB.Bson;

namespace SmartBin.Domain.Models
{
    public interface IEntity
    {
        ObjectId Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}
