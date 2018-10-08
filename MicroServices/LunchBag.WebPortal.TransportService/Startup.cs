using LunchBag.Common.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using LunchBag.Common.Managers;
using LunchBag.WebPortal.TransportService.Config;
using System.Net;

namespace LunchBag.WebPortal.TransportService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<AzureAdOptions>(Configuration.GetSection("AzureAd"));
            services.Configure<RestServiceOptions>(Configuration.GetSection("RestService"));
            services.Configure<TransportServiceOptions>(Configuration.GetSection("TransportService"));
            services.Configure<ServiceBusOptions>(Configuration.GetSection("ServiceBus"));
            services.Configure<AzureServiceBusOptions>(Configuration.GetSection("AzureServiceBus"));

            services.AddSingleton<ITransportRestService, TransportRestService>();
            services.AddSingleton<IAzureServiceBusClient, AzureServiceBusClient>();
            services.AddSingleton<IServiceBusClient, RabbitMQServiceBus>();
            services.AddSingleton<TransportService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.ApplicationServices.GetService<IServiceBusClient>().Start(null, string.Empty);
            app.ApplicationServices.GetService<TransportService>().Start();

            app.Run(context =>
            {
                return context.Response.WriteAsync("Transport Service is running...");
            });
        }
    }
}
