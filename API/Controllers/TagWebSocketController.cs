using Infrastructure.RabbitMq.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace API.Controllers
{
    [Route("Tag")]
    public class TagWebSocketController(
        TagSubscriptionService subscriptionService) : WebSocketControllerBase
    {
        private readonly TagSubscriptionService _subscriptionService = subscriptionService;

        public async Task<IActionResult> GetTagAsync(
            [FromQuery] bool isTemporary,
            CancellationToken cancellationToken)
        {
            return await HandleWebSocketRequestAsync(async webSocket =>
            {
                var buffer = new ArraySegment<byte>(new byte[4096]);
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                var tagIds = JsonConvert.DeserializeObject<int[]>(message);
             
                _subscriptionService.CreateQueueAndBind(
                    tagIds,
                    isTemporary,
                    async message =>
                    {
                        var response = Encoding.UTF8
                            .GetBytes(JsonConvert.SerializeObject(message));

                        await webSocket.SendAsync(
                            buffer: new ArraySegment<byte>(response),
                            messageType: WebSocketMessageType.Text,
                            endOfMessage: true,
                            cancellationToken: cancellationToken);
                    });

            },
            cancellationToken);
        }
    }
}
