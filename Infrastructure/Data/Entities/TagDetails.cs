using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Data.Entities;

public class TagDetails
{
    [BsonElement("name"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Name { get; set; }
    [BsonElement("url"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Url { get; set; }
}