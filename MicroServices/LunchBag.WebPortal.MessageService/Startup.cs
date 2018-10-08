using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LunchBag.Common;
using LunchBag.Common.Config;
using LunchBag.Common.Managers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LunchBag.WebPortal.MessageService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<AzureAdOptions>(Configuration.GetSection("AzureAd"));
            services.Configure<RestServiceOptions>(Configuration.GetSection("RestService"));
            services.Configure<RestServiceOptions>(Configuration.GetSection("MessageService"));
            services.Configure<ServiceBusOptions>(Configuration.GetSection("ServiceBus"));
            services.Configure<AzureServiceBusOptions>(Configuration.GetSection("AzureServiceBus"));

            services.AddSingleton<INoteRestService, NoteRestService>();
            services.AddSingleton<IEventRestService, EventRestService>();
            services.AddSingleton<IAzureServiceBusClient, AzureServiceBusClient>();
            services.AddSingleton<IServiceBusClient, RabbitMQServiceBus>();
            services.AddSingleton<MessageService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.ApplicationServices.GetService<IServiceBusClient>().Start(null, string.Empty);
            app.ApplicationServices.GetService<MessageService>().Start();

            app.Run(context =>
            {
                return context.Response.WriteAsync("Message Service is running...");
            });
        }
    }
}
