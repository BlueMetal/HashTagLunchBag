using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using LunchBag.WebPortal.Api.Helpers;
using LunchBag.Repositories;
using LunchBag.Repositories.Config;
using LunchBag.Common.Models;
using LunchBag.WebPortal.Api.Config;
using LunchBag.Common.Managers;
using LunchBag.Common.Models.Transport;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using LunchBag.WebPortal.Api.Validators;

namespace LunchBag.WebPortal.Api
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
            //Authentication
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = AzureADDefaults.BearerAuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = AzureADDefaults.JwtBearerAuthenticationScheme;
            })
            .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));

            //Options
            IConfigurationSection mobiliyaConfigSection = Configuration.GetSection("MobiliyaApiConfig");
            services.AddOptions();
            services.Configure<WebPortalApiConfig>(Configuration.GetSection("WebPortalApi"));
            services.Configure<MobiliyaApiConfig>(mobiliyaConfigSection);
            services.Configure<BingApiConfig>(Configuration.GetSection("BingApiConfig"));
            services.Configure<CosmosDBOptions>(typeof(EventModel).FullName, Configuration.GetSection("CosmosDb"));
            services.Configure<CosmosDBOptions>(typeof(EventModel).FullName, opt => opt.Collection = opt.Collections.Events);
            services.Configure<CosmosDBOptions>(typeof(NoteModel).FullName, Configuration.GetSection("CosmosDb"));
            services.Configure<CosmosDBOptions>(typeof(NoteModel).FullName, opt => opt.Collection = opt.Collections.Notes);
            services.Configure<CosmosDBOptions>(typeof(NoteTemplateModel).FullName, Configuration.GetSection("CosmosDb"));
            services.Configure<CosmosDBOptions>(typeof(NoteTemplateModel).FullName, opt => opt.Collection = opt.Collections.NoteTemplates);
            services.Configure<CosmosDBOptions>(typeof(DeliveryModel).FullName, Configuration.GetSection("CosmosDb"));
            services.Configure<CosmosDBOptions>(typeof(DeliveryModel).FullName, opt => opt.Collection = opt.Collections.Deliveries);
            services.Configure<CosmosDBOptions>(typeof(RouteModel).FullName, Configuration.GetSection("CosmosDb"));
            services.Configure<CosmosDBOptions>(typeof(RouteModel).FullName, opt => opt.Collection = opt.Collections.DeliveryRoutes);

            //DI
            MobiliyaApiConfig mobiliyaConfig = mobiliyaConfigSection.Get<MobiliyaApiConfig>();
            services.Configure<RedisCacheOptions>(redisOptions =>
            {
                redisOptions.Configuration = mobiliyaConfig.RedisConnection;
                redisOptions.InstanceName 
                    = mobiliyaConfig.RedisConnection.Substring(0, mobiliyaConfig.RedisConnection.IndexOf('.'));
            });
            services.AddTransient<IValidator<EventModel>, EventModelValidator>();
            services.AddTransient<IValidator<NoteModel>, NoteModelValidator>();
            services.AddTransient<IValidator<NoteTemplateModel>, NoteTemplateModelValidator>();
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddScoped(typeof(ICosmosDBRepository<>), typeof(CosmosDBRepository<>));
            services.AddScoped<EventDataManager>();
            services.AddScoped<NoteDataManager>();
            services.AddScoped<DeliveryDataManager>();
            services.AddScoped<MobiliyaDataManager>();
            services.AddScoped<RouteDataManager>();
            services.AddSignalR().AddRedis(Configuration.GetValue<string>("SignalR:ConnectionString"));
            services.AddScoped<DeliveryGeocoder>();

            //Cors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    );
            });

            //MVC
            services
                .AddMvc()
                .AddFluentValidation();

            //Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "LunchBag.MicroServices.WebApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //Enable Cors
            app.UseCors("CorsPolicy");

            //Enable SignalR
            app.UseSignalR(routes =>
            {
                routes.MapHub<SignalRHub>("/signalr");
            });

            //Swagger
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LunchBag.WebPortal.WebApi");
                });
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseMvc();
        }
    }
}
