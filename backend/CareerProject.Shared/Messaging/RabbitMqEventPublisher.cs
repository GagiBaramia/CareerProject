using System.Text;
using System.Text.Json;
using CareerProject.Shared.Events;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CareerProject.Shared.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    public const string ExchangeName = "career_project.events";

    private const int MaxAttempts = 3;
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(500),
    ];

    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly Lazy<Task<IConnection>> _connection;

    public RabbitMqEventPublisher(ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;
        _connection = new Lazy<Task<IConnection>>(() =>
            RabbitMqConnectionFactory.BuildFromEnvironment().CreateConnectionAsync());
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : EventBase
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, @event.GetType()));

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                var connection = await _connection.Value;
                await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
                await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
                await channel.BasicPublishAsync(ExchangeName, @event.RoutingKey, body, cancellationToken);

                return;
            }
            catch (Exception ex) when (attempt < MaxAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to publish {EventType} (attempt {Attempt}/{MaxAttempts}), retrying...",
                    typeof(TEvent).Name, attempt, MaxAttempts);
                await Task.Delay(RetryDelays[attempt - 1], cancellationToken);
            }
            catch (Exception ex)
            {
                // Event publishing is a side effect, not the primary operation (e.g. saving a
                // profile) - a broker outage must not fail the caller's request.
                _logger.LogError(
                    ex,
                    "Giving up publishing {EventType} (EventId {EventId}) after {MaxAttempts} attempts.",
                    typeof(TEvent).Name, @event.EventId, MaxAttempts);
                return;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection.IsValueCreated)
        {
            var connection = await _connection.Value;
            await connection.DisposeAsync();
        }
    }
}
