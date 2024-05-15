using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Data.Entities;

public class Album
{
    [BsonElement("artist"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Artist { get; set; }
    [BsonElement("title"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Title { get; set; }
    [BsonElement("mbid"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Mbid { get; set; }
    [BsonElement("url"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Url { get; set; }
    [BsonElement("image")]
    public ICollection<Image> Image { get; set; }
    [BsonElement("@attr")]
    public AlbumAttribute? Attribute { get; set; }
}