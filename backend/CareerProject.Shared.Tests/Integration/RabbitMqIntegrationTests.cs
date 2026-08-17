using CareerProject.Shared.Events;
using CareerProject.Shared.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CareerProject.Shared.Tests.Integration;

// Requires the real dev RabbitMQ (docker compose up -d, RABBITMQ_* env vars set) - proves the
// full publisher -> topic exchange -> queue -> consumer chain works, not just that each half
// compiles against the same contract.
public class RabbitMqIntegrationTests
{
    private const string TestRoutingKey = "test.integration.event";

    private sealed record TestEvent : EventBase
    {
        public override string RoutingKey => TestRoutingKey;
    }

    private sealed class TestConsumer(TaskCompletionSource<TestEvent> received, string queueName)
        : RabbitMqConsumerBase<TestEvent>(NullLogger.Instance)
    {
        protected override string QueueName => queueName;
        protected override string RoutingKey => TestRoutingKey;

        protected override Task HandleAsync(TestEvent @event, CancellationToken cancellationToken)
        {
            received.TrySetResult(@event);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task PublishedEvent_IsReceivedByABoundConsumer()
    {
        var queueName = $"test.queue.{Guid.NewGuid()}";
        var received = new TaskCompletionSource<TestEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var consumer = new TestConsumer(received, queueName);

        await consumer.StartAsync(CancellationToken.None);
        try
        {
            // Give the consumer a moment to declare/bind its queue before we publish -
            // otherwise the message could be published before the binding exists.
            await Task.Delay(TimeSpan.FromSeconds(1));

            await using var publisher = new RabbitMqEventPublisher(NullLogger<RabbitMqEventPublisher>.Instance);
            var sentEvent = new TestEvent { EntityId = Guid.NewGuid() };

            await publisher.PublishAsync(sentEvent);

            var receivedEvent = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));

            Assert.Equal(sentEvent.EventId, receivedEvent.EventId);
            Assert.Equal(sentEvent.EntityId, receivedEvent.EntityId);
        }
        finally
        {
            await consumer.StopAsync(CancellationToken.None);
            await DeleteQueueAsync(queueName);
        }
    }

    private static async Task DeleteQueueAsync(string queueName)
    {
        var factory = RabbitMqConnectionFactory.BuildFromEnvironment();
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeleteAsync(queueName, ifUnused: false, ifEmpty: false);
    }
}
