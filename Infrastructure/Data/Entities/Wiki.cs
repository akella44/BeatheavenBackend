using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Data.Entities;

public class Wiki
{
    [BsonElement("published"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Published { get; set; }
    [BsonElement("summary"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Summary { get; set; }
    [BsonElement("content"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Content { get; set; }
}