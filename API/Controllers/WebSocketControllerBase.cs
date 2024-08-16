using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class WebSocketControllerBase : ControllerBase
    {
        protected async Task<IActionResult> HandleWebSocketRequestAsync(
            Action<WebSocket> onWebSocketConnected,
            CancellationToken cancellationToken)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return BadRequest("WebSocket request required.");

            var webSocket = await HttpContext.WebSockets
                .AcceptWebSocketAsync();

            onWebSocketConnected(webSocket);

            await ReceiveWebSocketMessagesAsync(
                webSocket,
                cancellationToken);

            return new EmptyResult();
        }

        private static async Task ReceiveWebSocketMessagesAsync(
            WebSocket webSocket,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];

            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                cancellationToken);

            while (!result.CloseStatus.HasValue)
                result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

            await webSocket.CloseAsync(
                result.CloseStatus.Value,
                result.CloseStatusDescription,
                cancellationToken);
        }
    }
}
