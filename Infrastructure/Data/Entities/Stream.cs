using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Data.Entities;

public class Stream
{
    [BsonElement("#text"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Text { get; set; }
    [BsonElement("fulltrack"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string FullTrack { get; set; }
}