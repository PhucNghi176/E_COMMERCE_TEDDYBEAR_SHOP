namespace CONTRACT.CONTRACT.APPLICATION.DependencyInjection.Options;
public interface IMongoDbSettings
{
    string DatabaseName { get; set; }
    string ConnectionString { get; set; }
}

public class MongoDbSettings : IMongoDbSettings
{
    public required string DatabaseName { get; set; }
    public required string ConnectionString { get; set; }
}