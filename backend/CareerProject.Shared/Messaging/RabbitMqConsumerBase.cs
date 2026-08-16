using System.Text;
using System.Text.Json;
using CareerProject.Shared.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CareerProject.Shared.Messaging;

// Base class for a single-event-type consumer. A subclass declares which
// durable queue to bind to which routing key on the shared topic exchange,
// and implements HandleAsync - retries and basic error handling (nack
// without requeue after exhausting retries, so one bad message doesn't
// loop forever) are handled here.
public abstract class RabbitMqConsumerBase<TEvent> : BackgroundService where TEvent : EventBase
{
    private const int MaxHandleAttempts = 3;

    private readonly ILogger _logger;

    protected RabbitMqConsumerBase(ILogger logger)
    {
        _logger = logger;
    }

    protected abstract string QueueName { get; }
    protected abstract string RoutingKey { get; }

    protected abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await RabbitMqConnectionFactory.BuildFromEnvironment().CreateConnectionAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            RabbitMqEventPublisher.ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(
            QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(
            QueueName, RabbitMqEventPublisher.ExchangeName, RoutingKey, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            for (var attempt = 1; attempt <= MaxHandleAttempts; attempt++)
            {
                try
                {
                    var json = Encoding.UTF8.GetString(delivery.Body.ToArray());
                    var @event = JsonSerializer.Deserialize<TEvent>(json)
                        ?? throw new InvalidOperationException("Deserialized event was null.");

                    await HandleAsync(@event, stoppingToken);
                    await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, stoppingToken);
                    return;
                }
                catch (Exception ex) when (attempt < MaxHandleAttempts)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to handle {EventType} on queue {QueueName} (attempt {Attempt}/{MaxAttempts}), retrying...",
                        typeof(TEvent).Name, QueueName, attempt, MaxHandleAttempts);
                    await Task.Delay(TimeSpan.FromMilliseconds(300 * attempt), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Giving up handling {EventType} on queue {QueueName} after {MaxAttempts} attempts.",
                        typeof(TEvent).Name, QueueName, MaxHandleAttempts);
                    await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, stoppingToken);
                    return;
                }
            }
        };

        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown.
        }
    }
}
