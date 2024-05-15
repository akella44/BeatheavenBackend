using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Data.Entities;

public class Image
{
    [BsonElement("#text"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Text { get; set; }
    [BsonElement("size"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Size { get; set; }
}