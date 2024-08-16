using Infrastructure.RabbitMq.MassTransit;
using Infrastructure.RabbitMq.MassTransit.Models;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.RabbitMq.MassTransit
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMassTransit(
            this IServiceCollection services,
            RabbitMqConnectionModel rabbitMqConnectionModel)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqConnectionModel.Server, "/", h =>
                    {
                        h.Username(rabbitMqConnectionModel.Username);
                        h.Password(rabbitMqConnectionModel.Password);
                    });

                    cfg.AddHeaders(rabbitMqConnectionModel.Instance);

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }

    public static class RabbitMqBusFactoryConfiguratorExtensions
    {
        public static IPublishPipelineConfigurator AddHeaders(
            this IRabbitMqBusFactoryConfigurator cfg,
            string instanceName)
        {
            cfg.ConfigureSend(x => x.UseExecute(
                context => SetHeaders(
                    context,
                    instanceName)));

            cfg.ConfigurePublish(x => x.UseExecute(
                context => SetHeaders(
                    context,
                    instanceName)));

            return cfg;
        }

        private static void SetHeaders(SendContext context, string instanceName) =>
           context.Headers.Set("Instance", instanceName);
    }
}
