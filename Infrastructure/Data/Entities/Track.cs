using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Data.Entities;

public class Track
{
    [BsonId]
    [BsonElement("_id"), BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; }
    [BsonElement("name"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Name { get; set; }
    [BsonElement("mbid"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string? Mbid { get; set; }
    [BsonElement("url"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Url { get; set; }
    [BsonElement("duration"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Duration { get; set; }
    [BsonElement("streamable")]
    public Stream Stream { get; set; }
    [BsonElement("listeners"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Listeners { get; set; }
    [BsonElement("playcount"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Playcount { get; set; }
    [BsonElement("artist")]
    public Artist Artist { get; set; }
    [BsonElement("album")]
    public Album Album { get; set; }
    [BsonElement("toptags")]
    public Tag Tag { get; set; }
    [BsonElement("wiki")]
    public Wiki Wiki { get; set; }
    [BsonElement("youtube_url"), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string YoutubeUrl { get; set; }
}