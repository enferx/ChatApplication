using Microsoft.VisualBasic.FileIO;
using Rebus.Bus;
using Rebus.Handlers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebSocketEvents;

namespace BotParser
{
    public class CalculateStockPriceEventHandler : IHandleMessages<CalculateStockPriceEvent>
    {
        private readonly IBus _bus;

        public CalculateStockPriceEventHandler(IBus bus)
        {
            _bus = bus;
        }

        async Task IHandleMessages<CalculateStockPriceEvent>.Handle(CalculateStockPriceEvent message)
        {
            decimal quote = 0;
            bool isValid = true;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("text/csv"));
                var url = $"https://stooq.com/q/l/?s={message.Stock}&f=sd2t2ohlcv&h&e=csv";
                var response = await client.GetAsync(url).Result.Content.ReadAsStreamAsync(); //ReadAsStringAsync();
                using var parser = new TextFieldParser(response)
                {
                    TextFieldType = FieldType.Delimited
                };
                parser.SetDelimiters(",");
                string[] fields = parser.ReadFields();
                var index = Array.IndexOf(fields, "Close");
                while (!parser.EndOfData)
                {
                    fields = parser.ReadFields();
                    isValid = decimal.TryParse(fields[index], out quote);

                }
            }
            await _bus.Publish(new CalculatedStockPriceEvent(quote: quote, stock: message.Stock, webSocketId: message.WebSocketId, isValid: isValid));
        }
    }
}