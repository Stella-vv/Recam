namespace Remp.DataAccess.Settings;

public class MongoDbSettings
{
    public string ConnectionString {get; set; } = "";
    public string DatabaseName {get; set; } = "";
    public string LoginAttemptsCollectionName {get; set; } = "";
    public string RegisterAttemptsCollectionName {get; set; } = "";
}