using Remp.Models.Logs;
using Microsoft.Extensions.Options;
using Remp.DataAccess.Settings;
using MongoDB.Driver;

namespace Remp.DataAccess.Repositories;

public class MongoRegisterAttemptRepository : IRegisterAttemptRepository
{
    private readonly IMongoCollection<RegisterAttemptLog> _registerAttempts;

    public MongoRegisterAttemptRepository( IOptions<MongoDbSettings> mongoOptions )
    {
          MongoDbSettings settings = mongoOptions.Value;
          MongoClient client = new MongoClient(settings.ConnectionString);
          IMongoDatabase database = client.GetDatabase(settings.DatabaseName);
          _registerAttempts = database.GetCollection<RegisterAttemptLog>(settings.RegisterAttemptsCollectionName);

    }

    public Task AddAsync(RegisterAttemptLog log)
  {
      return _registerAttempts.InsertOneAsync(log);
  }


}