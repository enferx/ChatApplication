using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using System;
using WebSocketEvents;
using WebSocketManager;

namespace ChatApplication
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddWebSocketManager();
            services.AddMvc();

            var transportOptions = new SqlServerTransportOptions(
                @"Data Source = (LocalDb)\MSSQLLocalDB; Initial Catalog = RebusSource; Integrated Security = SSPI;");
            // possibly the default is true, so this line might not be needed
            transportOptions.SetEnsureTablesAreCreated(true);

            services.AddRebus((configure, provider) => configure
                .Logging(l => l.MicrosoftExtensionsLogging(
                    provider.GetRequiredService<ILoggerFactory>()))
                .Transport(t => t.UseSqlServer(transportOptions, "botparser"))
                .Routing(r => r.TypeBased().MapAssemblyOf<CalculatedStockPriceEvent>("botParser"))
                
                .Subscriptions(
                t => t.StoreInSqlServer(
                    @"Data Source = (LocalDb)\MSSQLLocalDB; Initial Catalog = RebusSource; Integrated Security = SSPI;", "Subscriptions", false, true
                    ))
                
                );
            services.AddRebusHandler<CalculatedStockPriceEventHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {

            app.UseWebSockets();
            app.MapWebSocketManager("/chat", serviceProvider.GetService<ChatHandler>());
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.ApplicationServices.UseRebus
                (async bus => await bus.Subscribe<CalculatedStockPriceEvent>());

        }
    }
}
