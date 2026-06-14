using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Favorite
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string UserId { get; set; }
    public string AdId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
