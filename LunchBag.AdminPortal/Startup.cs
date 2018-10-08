using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using LunchBag.AdminPortal.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using LunchBag.Common.Config;
using LunchBag.Common.Managers;
using LunchBag.AdminPortal.Config;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;

namespace LunchBag.AdminPortal
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
            //Authentication WebApp
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = AzureADDefaults.CookieScheme;
                sharedOptions.DefaultChallengeScheme = AzureADDefaults.OpenIdScheme;
            })
            .AddAzureAD(options => Configuration.Bind("AzureAd", options))
            .AddCookie();

            services.AddOptions();

            //Options
            services.Configure<AzureAdOptions>(Configuration.GetSection("AzureAd"));
            services.Configure<RestServiceOptions>(Configuration.GetSection("RestService"));
            services.Configure<RestRegistrationServiceOptions>(Configuration.GetSection("RestRegistrationService"));
            services.Configure<ServiceBusOptions>(Configuration.GetSection("ServiceBus"));
            services.Configure<AdminServiceOptions>(Configuration.GetSection("AdminService"));

            //DI
            services.AddSingleton<IEventRestService, EventRestService>();
            services.AddSingleton<ITransportRestService, TransportRestService>();
            services.AddSingleton<INoteRestService, NoteRestService>();
            services.AddSingleton<IRegistrationRestService, RegistrationRestService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IServiceBusClient, RabbitMQServiceBus>();
            services.AddSingleton<AdminService>();

            //MVC
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.ApplicationServices.GetService<IServiceBusClient>().Start(null, string.Empty);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseHttpsRedirection();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}