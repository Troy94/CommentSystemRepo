using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommentSystem.Presentation.Services
{
    public class CommentCreatedConsumer : BackgroundService
    {
        private readonly ILogger<CommentCreatedConsumer> _logger;
        private readonly IConfiguration _configuration;

        public CommentCreatedConsumer(ILogger<CommentCreatedConsumer> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private string ResolveHostName() => RabbitMqHostResolver.ResolveHostName(_configuration);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var hostName = ResolveHostName();
            var port = _configuration.GetValue<int>("RabbitMQ:Port", 5672);
            var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
            var password = _configuration["RabbitMQ:Password"] ?? "guest";
            var virtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/";
            var queueName = _configuration["RabbitMQ:QueueName"] ?? "comment-created-events";

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                VirtualHost = virtualHost,
                AutomaticRecoveryEnabled = true
            };

            try
            {
                var connection = await factory.CreateConnectionAsync(stoppingToken);
                var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, eventArgs) =>
                {
                    try
                    {
                        var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                        var message = JsonSerializer.Deserialize<CommentCreatedEvent>(payload);

                        if (message is null)
                        {
                            _logger.LogWarning("Received invalid RabbitMQ message from queue {QueueName}: {Payload}", queueName, payload);
                            await channel.BasicNackAsync(eventArgs.DeliveryTag, false, false, stoppingToken);
                            return;
                        }

                        _logger.LogInformation(
                            "Notification: comment created. CommentId={CommentId}, Text={Text}, CreatedAtUtc={CreatedAt}",
                            message.CommentId,
                            message.Text,
                            message.CreatedAt);

                        await channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing comment created event from queue {QueueName}", queueName);
                        await channel.BasicNackAsync(eventArgs.DeliveryTag, false, true, stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("RabbitMQ consumer started. Listening queue {QueueName}", queueName);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CommentCreatedConsumer is stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ consumer failed for queue {QueueName}", queueName);
            }
        }
    }

    public class CommentCreatedEvent
    {
        public string EventType { get; set; } = string.Empty;
        public int CommentId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
