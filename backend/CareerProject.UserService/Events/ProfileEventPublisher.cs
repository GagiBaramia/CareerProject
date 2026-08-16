using System.Text;
using System.Text.Json;
using CareerProject.Shared.Messaging;
using RabbitMQ.Client;

namespace CareerProject.UserService.Events;

// Minimal, direct publisher for ProfileCreated/ProfileUpdated only. Stage 13
// replaces this with the shared event-bus publisher/consumer abstraction
// used by every service - this is intentionally scoped to Task 7's needs.
public class ProfileEventPublisher : IAsyncDisposable
{
    private const string ExchangeName = "career_project.events";

    private readonly Lazy<Task<IConnection>> _connection;

    public ProfileEventPublisher()
    {
        _connection = new Lazy<Task<IConnection>>(() =>
            RabbitMqConnectionFactory.BuildFromEnvironment().CreateConnectionAsync());
    }

    public async Task PublishProfileCreatedAsync(Guid personProfileId) =>
        await PublishAsync("profile.created", personProfileId);

    public async Task PublishProfileUpdatedAsync(Guid personProfileId) =>
        await PublishAsync("profile.updated", personProfileId);

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
