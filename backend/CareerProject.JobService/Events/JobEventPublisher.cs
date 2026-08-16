using System.Text;
using System.Text.Json;
using CareerProject.Shared.Messaging;
using RabbitMQ.Client;

namespace CareerProject.JobService.Events;

// Minimal, direct publisher for JobCreated/JobUpdated only - same scoped
// approach as UserService's ProfileEventPublisher. Stage 13 replaces both
// with the shared publisher/consumer abstraction used across all services.
public class JobEventPublisher : IAsyncDisposable
{
    private const string ExchangeName = "career_project.events";

    private readonly Lazy<Task<IConnection>> _connection;

    public JobEventPublisher()
    {
        _connection = new Lazy<Task<IConnection>>(() =>
            RabbitMqConnectionFactory.BuildFromEnvironment().CreateConnectionAsync());
    }

    public async Task PublishJobCreatedAsync(Guid jobId) => await PublishAsync("job.created", jobId);

    public async Task PublishJobUpdatedAsync(Guid jobId) => await PublishAsync("job.updated", jobId);

    private async Task PublishAsync(string routingKey, Guid entityId)
    {
        var connection = await _connection.Value;
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true);

        var message = new
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EntityId = entityId,
        };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(ExchangeName, routingKey, body);
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
