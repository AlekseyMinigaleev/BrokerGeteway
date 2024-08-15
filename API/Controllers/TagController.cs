using Microsoft.AspNetCore.Mvc;
using MassTransit;
using Infrastructure.MassTransit.Consumers;
using System.Reflection.Metadata;

namespace API.Controllers
{
    //[ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    public class TagController(
        IBus bus) : ControllerBase
    {
        private readonly IBus _bus = bus;

        [HttpGet]
        public async Task GetTagAsync(int[] tagIds)
        {
            var queueName = string.Join(", ", tagIds);

            var handle = _bus.ConnectReceiveEndpoint(queueName, x =>
            {
                x.ConfigureConsumeTopology = false;

                x.Consumer<TagUpdatedValueConsumer>();
            });
            //if (HttpContext.WebSockets.IsWebSocketRequest)
            //{
            //    using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            //    _logger.LogInformation($"Sub with tags{queueName} websocket is open.");


            //}
            //else
            //{
            //    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            //}
        }
    }
}
