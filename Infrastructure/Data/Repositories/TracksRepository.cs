using Infrastructure.Data.DbContext;
using Infrastructure.Data.Entities;
using MongoDB.Driver;

namespace Infrastructure.Data.Repositories;

public class TracksRepository
{
    private readonly IMongoCollection<Track> _tracks;
    
    private readonly string _collection = Environment.GetEnvironmentVariable("MONGO_COLLECTION")!;
    
    public TracksRepository(AppDbContext appDbContext)
    {
        _tracks = appDbContext.Database.GetCollection<Track>(_collection);
    }

    public async Task<Track> GetById(string id)
    {
        try
        {
            FilterDefinition<Track> filterDefinition = Builders<Track>.Filter.Eq(t => t.Id, id);
            Track? track = await _tracks.Find(filterDefinition).FirstOrDefaultAsync();
            return track;
        }
        //mean incorrect id input (not 24 digit hex string)
        catch (System.FormatException exception)
        {
            return null;
        }
    }

    public async Task<string> CreateTrack(Track track)
    {
        await _tracks.InsertOneAsync(track);

        var filterBuilder = Builders<Track>.Filter;
        var filterDefinition = filterBuilder.Eq(t => t.Name, track.Name) 
                               & filterBuilder.Eq(t => t.Artist, track.Artist)
                               & filterBuilder.Eq(t => t.YoutubeUrl,track.YoutubeUrl);
        
        var projection = Builders<Track>.Projection.Include("_id");
        var resultEntity = await _tracks.Find(filterDefinition).Project(projection).FirstOrDefaultAsync();
        return resultEntity["_id"].AsObjectId.ToString();
    }
}