using MongoDB.Driver;

public class MongoService
{
    private readonly IMongoDatabase _db;

    public MongoService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        _db = client.GetDatabase(config["MongoDB:DatabaseName"]);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _db.GetCollection<T>(name);
    }
}
