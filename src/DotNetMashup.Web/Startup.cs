﻿using System.Collections.Generic;
using System.IO;
using DotNetMashup.Web.Factory;
using DotNetMashup.Web.Global;
using DotNetMashup.Web.Model;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Caching.Memory;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Newtonsoft.Json;

namespace DotNetMashup.Web
{
    public class Startup
    {
        private IEnumerable<IBlogMetaData> _feedData = null;
        private IConfiguration config = null;

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddApplicationInsightsSettings(instrumentationKey: "fda37cc5-e36e-4171-94ac-f02f9f1771c9")
            .Build();
            _feedData = JsonConvert.DeserializeObject<IEnumerable<BlogMetaData>>(File.ReadAllText(Path.Combine(appEnv.ApplicationBasePath, "blogfeed.json")));
        }

        //public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInstance(config);
            services.AddApplicationInsightsTelemetry(config);
            services.AddSingleton<ISiteSetting>(prov =>
            {
                return new SiteSettings();
            });
            services.AddSingleton<IMemoryCache>((provider) =>
            {
                return new MemoryCache(new MemoryCacheOptions());
            });
            services.AddCaching();
            services.AddInstance(_feedData);
            services.AddSingleton<RepositoryFactory>();
            // Add MVC services to the services container.
            services.AddMvc();

            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            // Configure the HTTP request pipeline.

            // Add the following to the request pipeline only in development environment.
            if(env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // send the request to the following path or controller action.
                app.UseExceptionHandler("/Home/Error")
                .UseApplicationInsightsRequestTelemetry()
                .UseApplicationInsightsExceptionTelemetry();
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles()
                .UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });
        }
    }
}