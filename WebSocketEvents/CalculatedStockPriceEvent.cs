using System;

namespace WebSocketEvents
{
    public class CalculatedStockPriceEvent
    {
        public string Stock { get; }

        public string WebSocketId { get; }

        public decimal Quote { get; }

        public CalculatedStockPriceEvent(string stock, string webSocketId, decimal quote)
        {
            Stock = stock;
            WebSocketId = webSocketId;
            Quote = quote;
        }
    }
}
