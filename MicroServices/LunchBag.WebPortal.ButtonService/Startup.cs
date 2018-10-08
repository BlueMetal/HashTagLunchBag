using LunchBag.Common.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using LunchBag.Common.Managers;
using System.Net;
using LunchBag.WebPortal.TransportService.Config;

namespace LunchBag.WebPortal.ButtonService
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
            services.Configure<ButtonServiceOptions>(Configuration.GetSection("ButtonService"));
            services.Configure<ServiceBusOptions>(Configuration.GetSection("ServiceBus"));
            services.Configure<AzureServiceBusOptions>(Configuration.GetSection("AzureServiceBus"));

            services.AddSingleton<IEventRestService, EventRestService>();
            services.AddSingleton<IAzureServiceBusClient, AzureServiceBusClient>();
            services.AddSingleton<IServiceBusClient, RabbitMQServiceBus>();
            services.AddSingleton<ButtonService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.ApplicationServices.GetService<IServiceBusClient>().Start(null, string.Empty);
            app.ApplicationServices.GetService<ButtonService>().Start();

            app.Run(context =>
            {
                return context.Response.WriteAsync("Button Service is running...");
            });
        }
    }
}
