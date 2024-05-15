using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Data.Entities;

public class Artist
{
    [BsonElement("name"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Name { get; set; }
    [BsonElement("mbid"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Mbid { get; set; }
    [BsonElement("url"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Url { get; set; }
}