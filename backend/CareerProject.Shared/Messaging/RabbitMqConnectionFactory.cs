using RabbitMQ.Client;

namespace CareerProject.Shared.Messaging;

// Builds a RabbitMQ ConnectionFactory from environment variables only -
// mirrors the RABBITMQ_* variables in .env, never hardcodes credentials.
public static class RabbitMqConnectionFactory
{
    public static ConnectionFactory BuildFromEnvironment()
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672";
        var username = Environment.GetEnvironmentVariable("RABBITMQ_USER")
            ?? throw new InvalidOperationException("RABBITMQ_USER environment variable is not set.");
        var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")
            ?? throw new InvalidOperationException("RABBITMQ_PASSWORD environment variable is not set.");

        return new ConnectionFactory
        {
            HostName = host,
            Port = int.Parse(port),
            UserName = username,
            Password = password,
        };
    }
}
