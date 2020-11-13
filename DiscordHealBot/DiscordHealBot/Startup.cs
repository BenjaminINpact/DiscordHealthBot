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
        protected List<string> Endpoints { get; set; }
        protected string DiscordWebHook { get; set; }

        public async Task RunAsync()
        {
            Logger.LogInformation($"Job Started at {DateTime.UtcNow:h:mm:ss tt zz}, {Settings.TimeInterval}s interval");
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                Logger.LogInformation("Job is looping");

                List<EndPointHealthResult> epResults = await RunEndPointsAsync();
                await BroadcastResultsAsync(epResults);
                await Task.Delay(Settings.GetTimeIntervalInMs());
            }
        }

        private async Task BroadcastResultsAsync(List<EndPointHealthResult> epResults)
        {
            var client = new DiscordWebhookClient(this.DiscordWebHook);
            Color embedColor = DecideEmbedColor(epResults);
            EmbedBuilder builder = new EmbedBuilder();
            var b =  builder.WithTitle("Latency Report")
                .WithDescription(ToDiscordDescription(epResults))
                .WithColor(embedColor)
                .Build();
       
       
            await client.SendMessageAsync(embeds: new List<Embed>() { b }, username:"Latency Bot");
          
        }

        private Color DecideEmbedColor(List<EndPointHealthResult> epResults)
        {
            if (epResults.Any(x => !x.Success) || epResults.Any(x => x.Latency > 10000))
            {
                return Color.Red;
            }

            if (epResults.Any(x => x.Latency > 2000))
            {
                return Color.Orange;
            }

            return Color.Green;
        }

        private string ToDiscordDescription(List<EndPointHealthResult> epResults)
        {
          //  
          var str = epResults.Select(x =>
          {
              return $"{x.EndpointAddress} responds with code {x.StatusCode} in {x.Latency}ms";
          }).ToList();

          return string.Join('\n', str);
        }

        private async Task<List<EndPointHealthResult>> RunEndPointsAsync()
        {
 
            List<EndPointHealthResult> endPointHealthResults =  new List<EndPointHealthResult>();
            foreach (string endpointAdress in Endpoints)
            {
                Stopwatch stopwatch = new Stopwatch();
                bool success = false;
                int statusCode = 0;
                stopwatch.Start();
                try
                {
                    var response = await endpointAdress.GetAsync();
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
                    EndpointAddress =  endpointAdress,
                    Success =  success,
                    StatusCode =  statusCode
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

            List<string> endpoints = configuration.GetSection("Endpoints").Get<List<string>>();
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