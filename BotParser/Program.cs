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
        
        static void Main(string[] args)
        {
            using (var activator = new BuiltinHandlerActivator())
            {
                activator.Register(() => new Handler());
                
                var transportOptions = new SqlServerTransportOptions(
                @"Data Source = (LocalDb)\MSSQLLocalDB; Initial Catalog = RebusSource; Integrated Security = SSPI;");

                Configure.With(activator)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
                    .Transport(t => t.UseSqlServer(transportOptions, "botParserExecutions"))
                    .Routing(r => r.TypeBased().MapAssemblyOf<CalculateStockPriceEvent>("botParserExecutions"))
                    .Subscriptions(x => x.StoreInSqlServer(@"Data Source = (LocalDb)\MSSQLLocalDB; Initial Catalog = RebusSource; Integrated Security = SSPI;", "Subscriptions", false, true))
                    .Start();
                activator.Bus.Subscribe<CalculateStockPriceEvent>().Wait();
            }
            
            while (true) ;
        }

        
    }

    public class Handler : IHandleMessages<CalculateStockPriceEvent>
    {
        private static readonly HttpClient client = new HttpClient();
        
        async Task IHandleMessages<CalculateStockPriceEvent>.Handle(CalculateStockPriceEvent message)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/csv"));
            var url = $"https://stooq.com/q/l/?s={message.Stock}&f=sd2t2ohlcv&h&e=csv";
            var response = await client.GetAsync(url).Result.Content.ReadAsStringAsync();
            // Falta enviar mensaje de vuelta al socket
            
        }
    }
}
