using Rebus.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WebSocketEvents;

namespace ChatApplication
{
    public class ChatHandler: WebSocketManager.WebSocketHandler
    {
        private readonly IBus _bus;

        public ChatHandler(WebSocketManager.WebSocketConnectionManager connectionManager, IBus bus) : base(connectionManager)
        {
            _bus = bus;
        }

        public override Task SendMessageAsync(WebSocket socket, string message, CancellationToken ct = default)
        {
            if (message.Contains(@"/stock="))
            {
                var stock = message.Split(@"/stock=").LastOrDefault();
                _bus.Publish(new CalculateStockPriceEvent(stock, GetIdBySocket(socket)));
            }
            return base.SendMessageAsync(socket, message, ct);
        }
    }
}
