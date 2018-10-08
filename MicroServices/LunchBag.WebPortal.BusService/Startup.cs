using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LunchBag.Common.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.AspNetCore.SignalR;
using LunchBag.Common.Managers;
using System.Net;

namespace LunchBag.WebPortal.BusService
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
            services.Configure<ServiceBusOptions>(Configuration.GetSection("ServiceBus"));
            services.AddMassTransit(c =>
            {
                c.AddConsumer<GoalUpdatedConsumer>();
                c.AddConsumer<NoteCreatedConsumer>();
                c.AddConsumer<SentimentUpdatedConsumer>();
                c.AddConsumer<ViewUpdatedConsumer>();
                c.AddConsumer<EventActiveStateChangedConsumer>();
                c.AddConsumer<DeliveryUpdatedConsumer>();
            });
            services.AddScoped<GoalUpdatedConsumer>();
            services.AddScoped<NoteCreatedConsumer>();
            services.AddScoped<SentimentUpdatedConsumer>();
            services.AddScoped<ViewUpdatedConsumer>();
            services.AddScoped<EventActiveStateChangedConsumer>();
            services.AddScoped<DeliveryUpdatedConsumer>();
            services.AddSingleton<ISignalsRestService, SignalsRestService>();
            services.AddSingleton<IServiceBusClient, RabbitMQServiceBus>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //Communication with the queue service
            app.ApplicationServices.GetService<IServiceBusClient>().Start(ep =>
            {
                ep.LoadFrom(app.ApplicationServices);
            }, "receive-message");
        }
    }
}
