using MongoDB.Bson;

namespace SmartBin.Api.GenericRepository
{
    public interface IEntity
    {
        ObjectId Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}
