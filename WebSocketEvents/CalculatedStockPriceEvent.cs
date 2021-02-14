using System;

namespace WebSocketEvents
{
    public class CalculatedStockPriceEvent
    {
        public string Stock { get; }

        public string WebSocketId { get; }

        public string Quote { get; }

        public CalculatedStockPriceEvent(string stock, string webSocketId, string quote)
        {
            Stock = stock;
            WebSocketId = webSocketId;
            Quote = quote;
        }
    }
}
