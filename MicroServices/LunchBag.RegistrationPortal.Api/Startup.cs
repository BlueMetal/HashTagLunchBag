using LunchBag.Common.Managers;
using LunchBag.Common.Models;
using LunchBag.RegistrationPortal.Api.Config;
using LunchBag.RegistrationPortal.Api.Helpers;
using LunchBag.RegistrationPortal.Api.Models;
using LunchBag.Repositories;
using LunchBag.Repositories.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;
using FluentValidation;
using LunchBag.RegistrationPortal.Api.Models.ApiMessages;
using LunchBag.RegistrationPortal.Api.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;

namespace LunchBag.RegistrationPortal.Api
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
            //Authentication
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = AzureADDefaults.BearerAuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = AzureADDefaults.JwtBearerAuthenticationScheme;
            })
            .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));

            services.AddOptions();

            //Options
            services.Configure<PayPalApiConfig>(Configuration.GetSection("PayPalApiConfig"));
            services.Configure<CosmosDBOptions>(typeof(RegistrationModel).FullName, Configuration.GetSection("CosmosDb"));
            services.Configure<CosmosDBOptions>(typeof(RegistrationModel).FullName, opt => opt.Collection = opt.Collections.Registrations);
            services.Configure<CosmosDBOptions>(typeof(EventModel).FullName, Configuration.GetSection("CosmosDb"));
            services.Configure<CosmosDBOptions>(typeof(EventModel).FullName, opt => opt.Collection = opt.Collections.Events);

            //DI
            services.AddTransient<IValidator<RegistrationCreatedMessage>, RegistrationCreatedMessageValidator>();
            services.AddTransient<IValidator<RegistrationDonationCreationMessage>, RegistrationDonationCreationMessageValidator>();
            services.AddTransient<IValidator<string>, RegistrationCheckUserValidator>();
            services.AddScoped(typeof(ICosmosDBRepository<>), typeof(CosmosDBRepository<>));
            services.AddScoped<PayPalHelper>();

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

            //Swagger
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LunchBag.RegistrationPortal.WebApi");
                });
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseMvc();
        }
    }
}
