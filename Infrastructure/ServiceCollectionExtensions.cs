using Infrastructure.RabbitMq;
using Infrastructure.RabbitMq.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var rabbitMqConnectionModel = configuration
                .GetSection("RabbitMqConnection")
                .Get<RabbitMqConnectionModel>()
                ?? throw new InvalidOperationException("RabbitMq connection is not configured properly.");

            services.AddRabbitMq(rabbitMqConnectionModel);

            return services;
        }
    }
}