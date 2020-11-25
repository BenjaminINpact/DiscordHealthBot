using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.Webhook;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordHealBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IServiceCollection serviceCollection)
        {
            ServiceProvider = serviceCollection.BuildServiceProvider();
            AssertConfiguration(configuration);

            using (var scope = ServiceProvider.CreateScope())
            {
                Logger = scope.ServiceProvider.GetService<ILogger<Startup>>();
            }

            this.CancellationTokenSource = new CancellationTokenSource();
        }


        protected ILogger Logger { get; }
        protected ServiceProvider ServiceProvider { get; }
        protected CancellationTokenSource CancellationTokenSource { get; }

        protected JobSettings Settings { get; set; }
        protected List<Endpoint> Endpoints { get; set; }
        protected string DiscordWebHook { get; set; }

        public async Task RunAsync()
        {
            Logger.LogInformation($"Job Started at {DateTime.UtcNow:h:mm:ss tt zz}, {Settings.TimeInterval}s interval");
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                Logger.LogInformation("Job is looping");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                List<EndPointHealthResult> epResults = await RunEndPointsAsync();
                await BroadCaster.BroadcastResultsAsync(epResults, this.DiscordWebHook, Settings.FamilyReporting);
                stopwatch.Stop();

                int result = Settings.GetTimeIntervalInMs() > (int)(stopwatch.ElapsedMilliseconds)
                    ? Settings.GetTimeIntervalInMs() - (int)(stopwatch.ElapsedMilliseconds)
                    : Settings.GetTimeIntervalInMs();

                await Task.Delay(result);
            }
        }

        private async Task<List<EndPointHealthResult>> RunEndPointsAsync()
        {
 
            List<EndPointHealthResult> endPointHealthResults =  new List<EndPointHealthResult>();
            foreach (Endpoint ep in Endpoints)
            {
                Stopwatch stopwatch = new Stopwatch();
                bool success = false;
                int statusCode = 0;
                stopwatch.Start();
                try
                {
                    var response = await ep.Address.GetAsync();
                    success = response.StatusCode > 199 && response.StatusCode < 400;
                    statusCode = response.StatusCode;
                }
                catch (Exception e)
                {
                    
                }
                stopwatch.Stop();

                EndPointHealthResult healthResult = new EndPointHealthResult()
                {
                    Latency = stopwatch.ElapsedMilliseconds,
                    EndpointAddress =  ep.Address,
                    Success =  success,
                    StatusCode =  statusCode,
                    Family =  ep.FamilyName
                };

                endPointHealthResults.Add(healthResult);
            }

            return endPointHealthResults.ToList();
        }

        protected void AssertConfiguration(IConfiguration configuration)
        {
            JobSettings settings = configuration.GetSection("JobSettings").Get<JobSettings>();
            if (settings == null || settings.TimeInterval < 1)
            {
                throw new InvalidDataException("JobParameters missing from appsettings or incorrect values");
            }

            Settings = settings;

            List<Endpoint> endpoints = configuration.GetSection("Endpoints").Get<List<Endpoint>>();
            if (endpoints == null || endpoints.Count < 1)
            {
                throw new InvalidDataException("No Endpoint to monitor, check your appsettings file");
            }

            Endpoints = endpoints;

            string discordWebHook = configuration.GetSection("DiscordWebhook").Get<string>();
            if (string.IsNullOrWhiteSpace(discordWebHook))
            {
                throw new InvalidDataException("No Discord Endpoint, check your appsettings file");           
            }

            DiscordWebHook = discordWebHook;




        }
    }
}