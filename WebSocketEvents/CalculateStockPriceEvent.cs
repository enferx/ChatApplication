using System;

namespace WebSocketEvents
{
    public class CalculateStockPriceEvent
    {
        public string Stock { get; }

        public string WebSocketId { get; }

        public CalculateStockPriceEvent(string stock, string webSocketId)
        {
            Stock = stock;
            WebSocketId = webSocketId;
        }
    }
}
