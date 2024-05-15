using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Data.Entities;

public class AlbumAttribute
{
    [BsonElement("position"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Position { get; set; }
}