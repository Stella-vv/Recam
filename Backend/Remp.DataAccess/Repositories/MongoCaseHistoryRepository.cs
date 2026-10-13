using Remp.Models.Logs;
using Microsoft.Extensions.Options;
using Remp.DataAccess.Settings;
using MongoDB.Driver;

namespace Remp.DataAccess.Repositories;

public class MongoCaseHistoryRepository : ICaseHistoryRepository
{
    private readonly IMongoCollection<CaseHistoryLog> _caseHistory;

    public MongoCaseHistoryRepository(IOptions<MongoDbSettings> mongoOptions)
    {
        MongoDbSettings settings = mongoOptions.Value;
        MongoClient client = new MongoClient(settings.ConnectionString);
        IMongoDatabase database = client.GetDatabase(settings.DatabaseName);
        _caseHistory = database.GetCollection<CaseHistoryLog>(settings.CaseHistoryCollectionName);
    }

    public Task AddAsync(CaseHistoryLog log)
    {
        return _caseHistory.InsertOneAsync(log);
    }
}