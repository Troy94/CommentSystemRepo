using System.Text;
using System.Text.Json;
using CommentSystem.Core.Entities;
using RabbitMQ.Client;

namespace CommentSystem.Presentation.Services
{
    public interface ICommentNotificationService
    {
        Task NotifyCommentCreatedAsync(Comment comment, CancellationToken cancellationToken = default);
    }

    public class CommentNotificationService : ICommentNotificationService
    {
        private readonly ILogger<CommentNotificationService> _logger;
        private readonly IConfiguration _configuration;

        public CommentNotificationService(ILogger<CommentNotificationService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task NotifyCommentCreatedAsync(Comment comment, CancellationToken cancellationToken = default)
        {
            var queueName = _configuration["RabbitMQ:QueueName"] ?? "comment-created-events";
            var hostName = RabbitMqHostResolver.ResolveHostName(_configuration);
            var port = _configuration.GetValue<int>("RabbitMQ:Port", 5672);
            var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
            var password = _configuration["RabbitMQ:Password"] ?? "guest";
            var virtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/";

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = hostName,
                    Port = port,
                    UserName = userName,
                    Password = password,
                    VirtualHost = virtualHost,
                    AutomaticRecoveryEnabled = true
                };

                using var connection = await factory.CreateConnectionAsync(cancellationToken);
                using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                var payload = JsonSerializer.Serialize(new
                {
                    EventType = "CommentCreated",
                    CommentId = comment.Id,
                    comment.Text,
                    comment.CreatedAt
                });

                var body = Encoding.UTF8.GetBytes(payload);

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    mandatory: false,
                    basicProperties: new BasicProperties { Persistent = true },
                    body: body,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Comment created event sent to RabbitMQ queue {QueueName}. CommentId {CommentId}", queueName, comment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send comment creation notification to RabbitMQ queue {QueueName}. CommentId {CommentId}", queueName, comment.Id);
            }
        }
    }
}
