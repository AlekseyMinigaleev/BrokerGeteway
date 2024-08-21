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

        [HttpGet("GetTags")]
        public async Task<IActionResult> GetTagsAsync(
            CancellationToken cancellationToken,
            [FromQuery] bool isTemporary = true)
        {
            return await HandleWebSocketRequestAsync(async webSocket =>
            {
                var tagIds = await GetTagIdsFromAsync(webSocket,cancellationToken);

                _subscriptionService.CreateQueueAndBind(
                    tagIds,
                    async message =>
                    {
                        var response = Encoding.UTF8
                            .GetBytes(JsonConvert.SerializeObject(message));

                        await webSocket.SendAsync(
                            buffer: new ArraySegment<byte>(response),
                            messageType: WebSocketMessageType.Text,
                            endOfMessage: true,
                            cancellationToken: cancellationToken);
                    },
                    isTemporary);

            },
            cancellationToken);
        }

        private static async Task<int[]?> GetTagIdsFromAsync(
            WebSocket webSocket,
            CancellationToken cancellationToken)
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);
            var result = await webSocket.ReceiveAsync(buffer, cancellationToken);
            if (buffer.Array is null)
                throw new InvalidOperationException(
                    "The buffer array is unexpectedly null. " +
                    "This indicates an issue with memory allocation " +
                    "or an unexpected state in the WebSocket data reception.");

            var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            var tagIds = JsonConvert.DeserializeObject<int[]>(message);

            return tagIds;
        }
    }
}
