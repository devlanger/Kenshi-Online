using Elasticsearch.Net;
using Nest;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Kenshi.API.Metrics;

public class NestLogger : ILogger
{
    private readonly IElasticClient _client;

    public NestLogger(IElasticClient client)
    {
        _client = client;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);
        var log = new Log(logLevel.ToString(), message, DateTime.Now);
        var indexResponse = _client.IndexDocument(log);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }
}

public record Log(string Level, string Message, DateTime Timestamp);
