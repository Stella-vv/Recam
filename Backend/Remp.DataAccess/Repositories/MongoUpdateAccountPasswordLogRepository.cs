using MongoDB.Driver;
using Remp.Models.Logs;
using Microsoft.Extensions.Options;
using Remp.DataAccess.Settings;

namespace Remp.DataAccess.Repositories;

public class MongoUpdateAccountPasswordLogRepository : IUpdateAccountPasswordLogRepository
{
    private readonly IMongoCollection<UpdateAccountPasswordLog> _updateAccountPasswordLogs;

    public MongoUpdateAccountPasswordLogRepository(IOptions<MongoDbSettings> mongoOptions)
    {
          MongoDbSettings settings = mongoOptions.Value;
          MongoClient client = new MongoClient(settings.ConnectionString);
          IMongoDatabase database = client.GetDatabase(settings.DatabaseName);
          _updateAccountPasswordLogs = database.GetCollection<UpdateAccountPasswordLog>(settings.UpdateAccountPasswordLogsCollectionName);

    }

    public Task AddAsync(UpdateAccountPasswordLog log)
    {
      return _updateAccountPasswordLogs.InsertOneAsync(log);
    }
}