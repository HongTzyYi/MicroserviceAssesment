// <copyright file="Program.cs" company="Grass Valley">
// Copyright (c) Grass Valley. All rights reserved.
// </copyright>

namespace GV.SCS.Store.FridgeStore
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GV.SCS.Platform.Interface;
    using GV.SCS.Platform.Interface.Host;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Main class for the GV Platform enabled service.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for the service.
        /// </summary>
        /// <param name="args">Arguments passed by command line parameters.</param>
        public static void Main(string[] args) =>
            RunAsync(CreateWebHostBuilder().Build()).GetAwaiter().GetResult();

        /// <summary>
        /// CreateWebHostBuilder
        /// </summary>
        /// <returns>IWebHostBuilder.</returns>
        public static IWebHostBuilder CreateWebHostBuilder() =>
            WebHost.CreateDefaultBuilder()
                .UseScsUrl()
                .ConfigureKestrel((context, options) =>
                {
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(3);
                    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
                })
                .UseStartup<Startup>();

        /// <summary>
        /// RunAsync
        /// </summary>
        /// <param name="host">host.</param>
        /// <param name="cancel">cancel.</param>
        /// <returns>Task.</returns>
        public static async Task RunAsync(IWebHost host, CancellationToken cancel = default(CancellationToken)) =>
            await host.Services.GetService<ILauncher>()
                .RunAsync(host, cancel);
    }
}
