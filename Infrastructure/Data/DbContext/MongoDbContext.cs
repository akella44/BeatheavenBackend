using System.Security.Authentication;
using MongoDB.Driver;
namespace Infrastructure.Data.DbContext;

public class MongoDbContext
{
    public IMongoDatabase Database { get; init; }

    private readonly string _mongoHost = Environment.GetEnvironmentVariable("MONGO_HOST")!;
    private readonly int _mongoPort = Convert.ToInt32(Environment.GetEnvironmentVariable("MONGO_PORT")!);
    private readonly string _database = Environment.GetEnvironmentVariable("MONGO_DB")!;
    private readonly string _username = Environment.GetEnvironmentVariable("MONGO_USER")!;
    private readonly string _password = Environment.GetEnvironmentVariable("MONGO_PWD")!;
    
    public MongoDbContext()
    {
        MongoClientSettings settings = new MongoClientSettings();
        settings.Server = new MongoServerAddress(_mongoHost, _mongoPort);
        settings.UseTls = false;
        settings.SslSettings = new SslSettings();
        settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;
        
        MongoIdentity identity = new MongoInternalIdentity(_database, _username);
        MongoIdentityEvidence evidence = new PasswordEvidence(_password);

        settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

        var mongoClient = new MongoClient(settings);
        Database = mongoClient.GetDatabase(_database);
    }
}