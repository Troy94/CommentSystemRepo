namespace CommentSystem.Presentation.Services
{
    internal static class RabbitMqHostResolver
    {
        public static string ResolveHostName(IConfiguration configuration)
        {
            var envHost =
                Environment.GetEnvironmentVariable("RabbitMQ__HostName") ??
                Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ??
                Environment.GetEnvironmentVariable("RABBITMQ_HOST");

            if (!string.IsNullOrWhiteSpace(envHost))
            {
                return envHost;
            }

            var configuredHost = configuration["RabbitMQ:HostName"];

            if (!string.IsNullOrWhiteSpace(configuredHost))
            {
                return configuredHost;
            }

            return "rabbitmq";
        }
    }
}