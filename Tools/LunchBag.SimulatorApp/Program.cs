using System;
using System.IO;
using System.Threading.Tasks;
using LunchBag.Common.Config;
using LunchBag.Common.Managers;
using LunchBag.Common.Models;
using LunchBag.Repositories;
using LunchBag.Repositories.Config;
using LunchBag.SimulatorApp.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LunchBag.SimulatorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            var app = serviceProvider.GetService<Application>();
            Task.Run(() => app.Run()).Wait();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            services.AddSingleton(loggerFactory);
            services.AddLogging();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            
            IConfigurationRoot configuration = builder.Build();
            services.AddSingleton(configuration);

            // Support typed Options
            services.AddOptions();
            services.Configure<SimulationConfig>(configuration.GetSection("Simulation"));
            services.Configure<CosmosDBOptions>(typeof(EventModel).FullName, configuration.GetSection("CosmosDb"));
            services.Configure<CosmosDBOptions>(typeof(EventModel).FullName, opt => opt.Collection = opt.Collections.Events);
            services.Configure<CosmosDBOptions>(typeof(NoteTemplateModel).FullName, configuration.GetSection("CosmosDb"));
            services.Configure<CosmosDBOptions>(typeof(NoteTemplateModel).FullName, opt => opt.Collection = opt.Collections.NoteTemplates);
            services.AddSingleton(new AzureServiceBusPublisher("buttons", configuration.GetSection("AzureServiceBusButtons").Get<AzureServiceBusOptions>()));
            services.AddSingleton(new AzureServiceBusPublisher("notes", configuration.GetSection("AzureServiceBusNotes").Get<AzureServiceBusOptions>()));
            services.AddScoped(typeof(ICosmosDBRepository<>), typeof(CosmosDBRepository<>));

            services.AddTransient<Application>();
        }
    }
}
