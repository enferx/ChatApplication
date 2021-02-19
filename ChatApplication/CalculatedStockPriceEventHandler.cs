using Rebus.Bus;
using Rebus.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketEvents;

namespace ChatApplication
{
    public class CalculatedStockPriceEventHandler : IHandleMessages<CalculatedStockPriceEvent>
    {
        private readonly ChatHandler _chatHandler;
        
        public CalculatedStockPriceEventHandler(ChatHandler handler)
        {
            _chatHandler = handler;
        }
        
        async Task IHandleMessages<CalculatedStockPriceEvent>.Handle(CalculatedStockPriceEvent message)
        {
            var socket = _chatHandler.GetById(message.WebSocketId);
            if (socket != null)
            {
                if (message.IsValid)
                {
                    await _chatHandler.SendMessageAsync(socket, $"{message.Stock} quote is {message.Quote} per share");
                }
                else
                {
                    await _chatHandler.SendMessageAsync(socket, "The specified stock doesn't exists or it couldn't be found.");
                }
            }
        }
    }
}
