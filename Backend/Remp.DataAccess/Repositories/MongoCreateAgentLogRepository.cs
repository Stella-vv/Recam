using Remp.Models.Logs;
using Microsoft.Extensions.Options;
using Remp.DataAccess.Settings;
using MongoDB.Driver;

namespace Remp.DataAccess.Repositories;

public class MongoCreateAgentLogRepository : ICreateAgentLogRepository
{
    private readonly IMongoCollection<CreateAgentLog> _createAgentLogs;

    public MongoCreateAgentLogRepository(IOptions<MongoDbSettings> mongoOptions)
    {
        MongoDbSettings settings = mongoOptions.Value;
        MongoClient client = new MongoClient(settings.ConnectionString);
        IMongoDatabase database = client.GetDatabase(settings.DatabaseName);
        _createAgentLogs = database.GetCollection<CreateAgentLog>(settings.CreateAgentLogsCollectionName);
    }

    public Task AddAsync(CreateAgentLog log)
    {
        return _createAgentLogs.InsertOneAsync(log);
    }
}