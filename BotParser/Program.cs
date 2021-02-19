using Microsoft.VisualBasic.FileIO;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Logging;
using Rebus.Routing.TypeBased;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebSocketEvents;

namespace BotParser
{
    class Program
    {

        public static BuiltinHandlerActivator _activator;

        static void Main(string[] args)
        {
            var transportOptions = new SqlServerTransportOptions(
                @"Data Source = (LocalDb)\MSSQLLocalDB; Initial Catalog = RebusSource; Integrated Security = SSPI;");
            transportOptions.SetEnsureTablesAreCreated(true);

            _activator = new BuiltinHandlerActivator();
            _activator.Register((x) => new Handler(_activator.Bus));
           
            Configure.With(_activator)
                .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
                .Transport(t => t.UseSqlServer(transportOptions, "botParserExecutions"))
                .Routing(r => r.TypeBased().MapAssemblyOf<CalculateStockPriceEvent>("botParserExecutions"))
                .Subscriptions(x => x.StoreInSqlServer(@"Data Source = (LocalDb)\MSSQLLocalDB; Initial Catalog = RebusSource; Integrated Security = SSPI;", "Subscriptions", false, true))
                .Start();
            _activator.Bus.Subscribe<CalculateStockPriceEvent>().Wait();
            while (true) ;
        }

        
    }

    public class Handler : IHandleMessages<CalculateStockPriceEvent>
    {
        private readonly IBus _bus;
        
        public Handler(IBus bus)
        {
            _bus = bus;
        }

        async Task IHandleMessages<CalculateStockPriceEvent>.Handle(CalculateStockPriceEvent message)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/csv"));
            var url = $"https://stooq.com/q/l/?s={message.Stock}&f=sd2t2ohlcv&h&e=csv";
            var response = await client.GetAsync(url).Result.Content.ReadAsStreamAsync(); //ReadAsStringAsync();
            decimal quote = 0;
            using (var parser = new TextFieldParser(response))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                string[] fields = parser.ReadFields();
                var index = Array.IndexOf(fields, "Close");
                while (!parser.EndOfData)
                {
                    fields = parser.ReadFields();
                    quote = Convert.ToDecimal(fields[index]);         
                }
            }
            await _bus.Publish(new CalculatedStockPriceEvent(quote: quote, stock: message.Stock, webSocketId: message.WebSocketId));
        }
    }
}
