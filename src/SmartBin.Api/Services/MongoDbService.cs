using SmartBin.Domain.Models;
using MongoDB.Driver;

namespace SmartBin.Api.Services
{
    public class MongoDbService
    {
        public IMongoCollection<User> Users { get; }
        public IMongoCollection<Bin> Bins { get; }

        public MongoDbService(IConfiguration config)
        {
            var mongo = config.GetSection("MongoDB");
            var client = new MongoClient(mongo["ConnectionString"]);
            var db = client.GetDatabase(mongo["DatabaseName"]);

            Users = db.GetCollection<User>(mongo["UsersCollection"]);
            Bins = db.GetCollection<Bin>(mongo["BinsCollection"]);
        }
    }
}
