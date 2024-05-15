using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Data.Entities;

public class Tag
{
    [BsonElement("tag")]
    public ICollection<TagDetails> Tags { get; set; }
}