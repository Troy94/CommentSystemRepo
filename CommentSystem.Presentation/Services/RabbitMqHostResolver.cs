namespace CommentSystem.Presentation.Services
{
    internal static class RabbitMqHostResolver
    {
        public static string ResolveHostName(IConfiguration configuration)
        {
            var envHost = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME")
                ?? Environment.GetEnvironmentVariable("RABBITMQ_HOST")
                ?? Environment.GetEnvironmentVariable("RabbitMQ__HostName");

            if (!string.IsNullOrWhiteSpace(envHost))
            {
                return envHost;
            }

            var configuredHost = configuration["RabbitMQ:HostName"] ?? "localhost";

            if (IsRunningInContainer() && string.Equals(configuredHost, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                return Environment.GetEnvironmentVariable("RABBITMQ_CONTAINER_HOST") ?? "host.docker.internal";
            }

            return configuredHost;
        }

        public static bool IsRunningInContainer()
        {
            var value = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
