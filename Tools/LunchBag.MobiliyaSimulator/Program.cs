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
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace LunchBag.MobiliyaSimulator
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
            services.Configure<RestServiceOptions>(configuration.GetSection("RestService"));
            services.Configure<AzureAdOptions>(configuration.GetSection("AzureAd"));

            services.AddTransient<Application>();
        }
    }
}
