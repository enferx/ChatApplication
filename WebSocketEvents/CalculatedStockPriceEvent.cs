using System;

namespace WebSocketEvents
{
    public class CalculatedStockPriceEvent
    {
        public string Stock { get; }

        public string WebSocketId { get; }

        public decimal Quote { get; }

        public bool IsValid { get; set; }

        public CalculatedStockPriceEvent(string stock, string webSocketId, decimal quote, bool isValid)
        {
            Stock = stock;
            WebSocketId = webSocketId;
            Quote = quote;
            IsValid = isValid;
        }
    }
}
