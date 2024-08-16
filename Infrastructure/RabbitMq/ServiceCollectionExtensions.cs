using Infrastructure.RabbitMq.MassTransit.Models;
using Infrastructure.RabbitMq.Services;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Infrastructure.RabbitMq
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMq(
           this IServiceCollection services,
           RabbitMqConnectionModel rabbitMqConnectionModel)
        {
            services.AddTransient(provider =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = rabbitMqConnectionModel.Server,
                    UserName = rabbitMqConnectionModel.Username,
                    Password = rabbitMqConnectionModel.Password
                };
                return factory;
            });

            services.AddTransient(provider =>
            {
                var factory = provider
                    .GetRequiredService<ConnectionFactory>();
                return factory.CreateConnection();
            });

            services.AddTransient<TagSubscriptionService>();

            return services;
        }
    }
}
