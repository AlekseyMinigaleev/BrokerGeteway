using Infrastructure.MassTransit.Messages;
using MassTransit;

namespace Infrastructure.MassTransit.Consumers
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
