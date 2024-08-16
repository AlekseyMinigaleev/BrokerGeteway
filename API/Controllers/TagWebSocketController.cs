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

        [HttpGet]
        public async Task<IActionResult> GetTagAsync(
            [FromQuery] int[] tagIds,
            CancellationToken cancellationToken)
        {
            return await HandleWebSocketRequestAsync(webSocket =>
            {
                _subscriptionService.CreateQueueAndBind(tagIds, async message =>
                {
                    var response = Encoding.UTF8
                        .GetBytes(JsonConvert.SerializeObject(message));

                    await webSocket.SendAsync(
                        buffer: new ArraySegment<byte>(response),
                        messageType: WebSocketMessageType.Text,
                        endOfMessage: true,
                        cancellationToken: cancellationToken);
                });

            }, cancellationToken);
        }
    }
}
