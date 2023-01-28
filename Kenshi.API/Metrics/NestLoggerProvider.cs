using Nest;

namespace Kenshi.API.Metrics;

public class NestLoggerProvider : ILoggerProvider
{
    private readonly IElasticClient _client;

    public NestLoggerProvider(Uri uri)
    {
        var connectionSettings = new ConnectionSettings(uri).DefaultIndex("default_index");
        _client = new ElasticClient(connectionSettings);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new NestLogger(_client);
    }

    public void Dispose() { }
}