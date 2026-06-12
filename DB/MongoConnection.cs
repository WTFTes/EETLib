using MongoDB.Driver;

namespace EETReader.DB;

public class MongoConnection(string uri, string databaseName)
{
    private static readonly Dictionary<string, MongoClient> Clients = new();
    
    private MongoClient GetConnection()
    {
        if (Clients.TryGetValue(uri, out var client))
            return client;
        
        client = new MongoClient(uri);
        
        Clients.Add(uri, client);

        return client;
    }

    private IMongoDatabase Database => GetConnection().GetDatabase(databaseName);

    public IMongoCollection<T> GetCollection<T>(string collectionName) => Database.GetCollection<T>(collectionName);
}
