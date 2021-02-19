using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Routing.TypeBased;
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
            _activator.Register((x) => new CalculateStockPriceEventHandler(_activator.Bus));
           
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
}
