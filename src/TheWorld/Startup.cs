﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheWorld.Models;
using TheWorld.Services;

namespace src {
    public class Startup {
        private IHostingEnvironment _env;
        private IConfigurationRoot _config;

        public Startup (IHostingEnvironment env) {
            _env = env;

            var builder = new ConfigurationBuilder ()
                .SetBasePath (_env.ContentRootPath)
                .AddJsonFile ("config.json")
                .AddJsonFile ($"secrets.json", optional : true)
                .AddEnvironmentVariables ();

            _config = builder.Build ();
        }

        public void ConfigureServices (IServiceCollection services) {
            services.AddSingleton (_config);

            if (_env.IsEnvironment ("Development") || _env.IsEnvironment ("Remote") || _env.IsEnvironment ("Testing")) {
                services.AddScoped<IMailService, DebugMailService> ();
            } else {
                //Implement a real Mail Service
            }
            services.AddDbContext<WorldContext> ();
            //services.AddTransient<WorldContextSeedData> ();
            // _context.Database.Migrate()
            // Database.EnsureCreated();

            services.AddMvc ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsEnvironment ("Development") || env.IsEnvironment ("Remote") || env.IsEnvironment ("Testing")) {
                app.UseDeveloperExceptionPage ();
            }
            //app.UseDefaultFiles (); //Feeds index.html into UseStaticFiles call
            app.UseStaticFiles (); //These two don't work in reverse

            try {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory> ()
                    .CreateScope ()) {

                    serviceScope.ServiceProvider.GetService<WorldContext> ()
                        .Database.EnsureCreated ();
                }
            } catch (Exception e) {
                var msg = e.Message;
                var stacktrace = e.StackTrace;
            }

            app.UseMvc (config => {
                config.MapRoute (
                    name: "Default",
                    template: "{controller}/{action}/{id?}", //id? doesn't have to exist 
                    defaults : new { controller = "App", action = "Index" } //action = "Index"; to specify which method 
                );
            });
        }
    }
}