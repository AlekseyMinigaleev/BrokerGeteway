using Infrastructure.RabbitMq.MassTransit.Messages;
using MassTransit;

namespace Infrastructure.RabbitMq.MassTransit.Consumers
{
    public class TagUpdatedValueConsumer : IConsumer<TagUpdatedValueMessage>
    {
        public Task Consume(ConsumeContext<TagUpdatedValueMessage> context)
        {
            Console.WriteLine(context.Message);

            return Task.CompletedTask;
        }
    }
}
