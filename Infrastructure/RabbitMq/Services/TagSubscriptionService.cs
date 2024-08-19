using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;
using Infrastructure.RabbitMq.MassTransit.Messages;

namespace Infrastructure.RabbitMq.Services
{
    public class TagSubscriptionService(IConnection connection)
    {
        private const string ExchangeName = "Agent.Infrastructure.MassTransit.Messages:TagUpdatedValueMessage"; /*TODO*/

        private readonly IConnection _connection = connection;

        private IModel _channel;
        private string _queueName;

        public string CreateQueueAndBind(
            int[] tagIds,
            bool isTemporary,
            Action<TagUpdatedValueMessage> onMessageReceived)
        {
            _channel = _connection.CreateModel();

            DeclareQueueAndExchange(isTemporary);
            BindQueueToExchange(tagIds);
            ConsumeMessages(onMessageReceived);

            return _queueName;
        }

        private void DeclareQueueAndExchange(bool isTemporary)
        {
            if (isTemporary)
            {
                _queueName = _channel.QueueDeclare().QueueName;
            }
            else
            {
                _queueName = "tag_value_updated_permanent_queue";
                _channel.QueueDeclare(
                    queue: _queueName,
                    durable: true,
                    autoDelete:false
                    );
            }

            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Direct,
                durable: false,
                autoDelete: true);
        }

        private void BindQueueToExchange(
            int[] tagIds)
        {
            foreach (var tagId in tagIds)
                _channel.QueueBind(
                    queue: _queueName,
                    exchange: ExchangeName,
                    routingKey: tagId.ToString());
        }

        private void ConsumeMessages(Action<TagUpdatedValueMessage> onMessageReceived)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
                onMessageReceived(DeserializeObject(ea));

            _channel.BasicConsume(
                queue: _queueName,
                autoAck: true,
                consumer: consumer);
        }

        private TagUpdatedValueMessage DeserializeObject(BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var jObject = JObject.Parse(json);

            var messageJObject = jObject["message"]
                ?? throw new InvalidOperationException("The message does not contain a 'message' field or it is null.");

            var tagValueUpdatedMessage = JsonConvert
                .DeserializeObject<TagUpdatedValueMessage>(messageJObject.ToString())
                ?? throw new InvalidOperationException($"Failed to deserialize the 'message' field into {nameof(TagUpdatedValueMessage)}.");

            return tagValueUpdatedMessage;
        }
    }
}