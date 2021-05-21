// <copyright file="Startup.cs" company="Grass Valley">
// Copyright (c) Grass Valley. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Reflection;
using GV.SCS.Platform.Interface;
using GV.SCS.Platform.Interface.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Morcatko.AspNetCore.JsonMergePatch;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System.Collections.Generic;

#pragma warning disable SA1210, SA1310, SA1507

namespace GV.SCS.Store.FridgeStore
{
    /// <summary>
    /// Startup configuration for SCS compatible MVC service.
    /// </summary>
    public class Startup :
        StartupForMvc
    {
        private const string INGRESS_EVENTLOG_SVC_HOST = "http://scs-eventlog-interface-service.scs";
        private const string INGRESS_LOGSVC_HOST = "http://gvp-logging-service.gvplatform:6005";


        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Startup configuration for attached services.</param>
        public Startup(IConfiguration configuration)
            : base(configuration)
        {
        }

        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            services.AddResponseCompression();

            // for background logging use
            // services.AddSingleton<ILoggerTaskQueue, LoggerTaskQueue>();
            // services.AddHostedService<LoggerBackgroundService>();

            // for any background-task
            // services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            // services.AddHostedService<QueuedBackgroundService>();


            services
                .AddSwaggerGen(c =>
                {
                    c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Description = "Authorisation using OAuth2 API key. Example: \"bearer {token}\"",
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });
                    c.OperationFilter<SecurityRequirementsOperationFilter>();
                    c.OperationFilter<AddResponseHeadersFilter>();

                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Test.Store.Fruit",
                        Version = "v1",
                        Description = "Fruit storage API (Fridge) to create, update or get fruit data"
                    });
                    c.IncludeXmlComments(xmlPath);
                })
                .AddHealthChecks()
                ;


            // Add services for dependency injection here.
            var sb = new System.Text.StringBuilder();
            foreach (var kv in Configuration.AsEnumerable())
            {
                sb.AppendLine($"\tKey: {kv.Key} Value: {kv.Value}");
            }

            Console.WriteLine($"Configuration:\n{sb.ToString()}");

            var useStableName = bool.Parse(Configuration.GetValue<string>("GVSCS_USE_STABLENAME") ?? "true");
            var eventLogStableAddr = useStableName ? INGRESS_EVENTLOG_SVC_HOST : Configuration.GetValue<string>("GVPLATFORM_ADDRESS");
            var loggingStableAddr = useStableName ? INGRESS_LOGSVC_HOST : Configuration.GetValue<string>("GVPLATFORM_ADDRESS");
            Console.WriteLine($"EventLog host: {eventLogStableAddr}");
            Console.WriteLine($"GVP Logging host: {loggingStableAddr}");


            // Add services for dependency injection here.
            services
                .AddMvc()
                .AddNewtonsoftJsonMergePatch();

            services.AddLogging(builder =>
                builder.AddConsole());

            // Project services

            // check threads
            // int workerThreadCount;
            // int ioThreadCount;
            // ThreadPool.GetMinThreads(out workerThreadCount, out ioThreadCount);
            // Console.WriteLine("Default min worker thread: " + workerThreadCount);
            // Console.WriteLine("Default min I/O thread: " + ioThreadCount);

            // ThreadPool.GetMaxThreads(out workerThreadCount, out ioThreadCount);
            // Console.WriteLine("Default max worker thread: " + workerThreadCount);
            // Console.WriteLine("Default max I/O thread: " + ioThreadCount);
        }

        /// <inheritdoc/>
        public override void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IOptions<EnvironmentConfiguration> envConfig,
            IOptions<GeneralConfiguration> generalConfig)
        {
            app.UseResponseCompression();

            base.Configure(app, env, envConfig, generalConfig);
            app
                .UseStaticFiles()
                .UseSwagger()
                .UseSwaggerUI(c =>
                {
                    //c.SwaggerEndpoint("/swagger-original.json", "GV.SCS.Store.FridgeStore"); //use swagger generated static page
                    c.SwaggerEndpoint("v1/swagger.json", "GV.SCS.Store.FridgeStore"); //use controller generated page, link wont work
                })
                .UseHealthChecks(
                    "/healthy",
                    new HealthCheckOptions { Predicate = (_) => false })
                .UseHealthChecks(
                    "/ready",
                    new HealthCheckOptions { Predicate = r => r.Tags.Contains("services") });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Perform configuration of the app builder here.
        }

    }
}
