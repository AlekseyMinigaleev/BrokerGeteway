using Infrastructure.MassTransit.Messages;
using Infrastructure.MassTransit.Models;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Infrastructure.Extensions
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransit(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var rabbitMqConnectionModel = configuration
                .GetSection("RabbitMqConnection")
                .Get<RabbitMqConnectionModel>()
                ?? throw new InvalidOperationException("RabbitMq connection is not configured properly.");

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqConnectionModel.Server, "/", h =>
                    {
                        h.Username(rabbitMqConnectionModel.Username);
                        h.Password(rabbitMqConnectionModel.Password);
                    });

                    cfg.AddTagUpdatedValueExchange();

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

        public static IPublishPipelineConfigurator AddTagUpdatedValueExchange(
            this IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Publish<TagUpdatedValueMessage>(x =>
            {
                x.Durable = false;
                x.ExchangeType = ExchangeType.Direct;
            });

            cfg.Send<TagUpdatedValueMessage>(x =>
            {
                x.UseRoutingKeyFormatter(context => context.Message.Id.ToString());
            });

            return cfg;
        }

        private static void SetHeaders(SendContext context, string instanceName) =>
           context.Headers.Set("Instance", instanceName);
    }
}
