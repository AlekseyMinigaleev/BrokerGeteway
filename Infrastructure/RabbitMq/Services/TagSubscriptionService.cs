using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace Infrastructure.RabbitMq.Services
{
    public class TagSubscriptionService(IConnection connection)
    {
        private const string ExchangeName = "tag_value_updated"; /*TODO*/

        private readonly IConnection _connection = connection;

        private IModel _channel;
        private string _queueName;

        public string CreateQueueAndBind(
            int[]? tagIds,
            Action<TagValueUpdated> onMessageReceived,
            bool isTemporary = true)
        {
            _channel = _connection.CreateModel();

            DeclareQueueAndExchange(isTemporary);
            BindQueueToExchange(tagIds);
            ConsumeMessages(onMessageReceived);

            return _queueName;
        }

        private void DeclareQueueAndExchange(
            bool isTemporary)
        {
            if (isTemporary)
            {
                _queueName = _channel.QueueDeclare().QueueName;
            }
            else
            {
                var guid = Guid.NewGuid();
                _queueName = $"tag_value_updated_{guid}";
                _channel.QueueDeclare(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);
            }

            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Direct,
                durable: false,
                autoDelete: true);
        }

        private void BindQueueToExchange(
            int[]? tagIds)
        {
            foreach (var tagId in tagIds)
                _channel.QueueBind(
                    queue: _queueName,
                    exchange: ExchangeName,
                    routingKey: tagId.ToString());
        }

        private void ConsumeMessages(Action<TagValueUpdated> onMessageReceived)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
                onMessageReceived(DeserializeObject(ea));

            _channel.BasicConsume(
                queue: _queueName,
                autoAck: true,
                consumer: consumer);
        }

        private static TagValueUpdated DeserializeObject(BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();

            var tagValueUpdated = TagValueUpdated.Parser.ParseFrom(body);
            return tagValueUpdated;
        }
    }
}