using Remp.Models.Logs;
using Microsoft.Extensions.Options;
using Remp.DataAccess.Settings;
using MongoDB.Driver;
namespace Remp.DataAccess.Repositories;

public class MongoLoginAttemptRepository: ILoginAttemptRepository
{
    private readonly IMongoCollection<LoginAttemptLog> _loginAttempts;

    public MongoLoginAttemptRepository(IOptions<MongoDbSettings> mongoOptions)
    {
        MongoDbSettings settings = mongoOptions.Value;
        MongoClient client = new MongoClient(settings.ConnectionString);
        IMongoDatabase database = client.GetDatabase(settings.DatabaseName);
        _loginAttempts = database.GetCollection<LoginAttemptLog>(settings.LoginAttemptsCollectionName);
    }

    public Task AddAsync (LoginAttemptLog log)
    {
        return _loginAttempts.InsertOneAsync(log);
    }
}