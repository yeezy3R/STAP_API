using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Name { get; set; }   // Nome da categoria (Carros, Motos, Eletrónica...)
    public string Icon { get; set; }   // Opcional: URL de ícone
}
