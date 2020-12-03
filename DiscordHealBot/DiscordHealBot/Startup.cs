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

            this.QueueResults = new ConcurrentQueue<EndPointHealthResult>();
            this.CancellationTokenSource = new CancellationTokenSource();
        }


        protected ILogger Logger { get; }
        protected ServiceProvider ServiceProvider { get; }
        protected CancellationTokenSource CancellationTokenSource { get; }

        protected JobSettings Settings { get; set; }
        protected List<Endpoint> Endpoints { get; set; }
        protected string DiscordWebHook { get; set; }

        protected ConcurrentQueue<EndPointHealthResult> QueueResults { get; set; }

        public async Task RunAsync()
        {
            Logger.LogInformation($"Job Started at {DateTime.UtcNow:h:mm:ss tt zz}, {Settings.TimeInterval}s interval");
            //stockage date
            Task pollingTask = PollAsync();
            Task repotingTask = ReportAsync();
            await Task.WhenAll(pollingTask, repotingTask);
            
        }

        private async Task ReportAsync()
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                Logger.LogInformation($"Job Started at {DateTime.UtcNow:h:mm:ss tt zz}, {Settings.TimeInterval}s interval");

                Logger.LogInformation("Job is announcing ..." + QueueResults.Count());
                List<EndPointHealthResult> epResults = new List<EndPointHealthResult>();
                while (QueueResults.TryDequeue(out var endPointHealthResult))
                {
                    epResults.Add(endPointHealthResult);
                }

                if (epResults.Count > 0)
                {
                    await BroadCaster.BroadcastResultsAsync(epResults, this.DiscordWebHook, Settings.FamilyReporting);
                    // update date de la derniere entrée
                }
                

                TimeSpan delay = new TimeSpan();
                   //vider liste endpointshealhresult bdd
                if(Settings.FixedTime)
                {
                    switch (Settings.TimeUnit)
                    {
                        case "day" :
                            delay = (DateTime.Now.AddDays(1).Date - DateTime.Now);
                            break;
                        default:
                        case "hour" :
                            delay = TimeSpan.FromMinutes(60 - DateTime.Now.Minute);
                            break;
                        case "minute":
                            delay = TimeSpan.FromSeconds(60 - DateTime.Now.Second);
                            break;
                    }
                } else
                {
                    //if(plusieurs rapport en bdd) 
                    //      delay = (date[0] + timeinterval) - date.now;
                    //      vider date[0];
                    //else
                    delay = TimeSpan.FromMilliseconds(Settings.GetAnnouncementTimeIntervalInMs());
                    
                }

                await Task.Delay(delay);
            }
        }

        private async Task PollAsync()
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                Logger.LogInformation("Job is polling ..." + QueueResults.Count());
                List<EndPointHealthResult> epResults = await RunEndPointsAsync();
                //getListEndpointsResult
                  // if(list.count > 0)
                     //  boucle pour add à la queue
                     // vider table Endpointresult
                foreach (EndPointHealthResult endPointHealthResult in epResults)
                {
                    QueueResults.Enqueue(endPointHealthResult);
                }

                await TrySendAlertAsync();
                await Task.Delay(Settings.GetPollingTimeIntervalInMs());
            }
        }

        private async Task TrySendAlertAsync()
        {
            if (Settings.SendAlert)
            {
                var exceeded = QueueResults.Where(x => x.Latency > Settings.AlertFloor).ToList();
                if (exceeded.Count > 0)
                {
                   await BroadCaster.BroadcastAlertAsync(exceeded, DiscordWebHook);
                }
            }
        }

        private async Task<List<EndPointHealthResult>> RunEndPointsAsync()
        {
            List<EndPointHealthResult> endPointHealthResults = new List<EndPointHealthResult>();
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
                    EndpointAddress = ep.Address,
                    Success = success,
                    StatusCode = statusCode,
                    Family = ep.FamilyName,
                    DateRun = DateTime.UtcNow
                };

                endPointHealthResults.Add(healthResult);
                //creation d'un endpointresult en bdd
            }

            return endPointHealthResults.ToList();
        }

        protected void AssertConfiguration(IConfiguration configuration)
        {
            JobSettings settings = configuration.GetSection("JobSettings").Get<JobSettings>();
            if (settings == null || settings.TimeInterval < 1 || settings.PollingInterval < 1)
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