using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;

namespace DiscordHealBot
{
    public static class BroadCaster
    {
        public static async Task BroadcastResultsAsync(List<EndPointHealthResult> epResults, string discordWebhook,
            bool family = false)
        {
            var client = new DiscordWebhookClient(discordWebhook);
            List<Embed> embeds = null;
            if (!family)
            {
                embeds = CreateClassicEmbeds(epResults);
            }
            else
            {
                embeds = CreateFamilyEmbeds(epResults);
            }

            await client.SendMessageAsync(embeds: embeds, username: "Latency Bot");
        }

        public static async Task BroadcastAlertAsync(List<EndPointHealthResult> epResults, string discordWebhook)
        {
            var client = new DiscordWebhookClient(discordWebhook);
            Color embedColor = Color.Red;
            EmbedBuilder builder = new EmbedBuilder();
            var description = epResults.Select(x =>
            {
                return $"⚠ : {x.EndpointAddress} responds with code {x.StatusCode} in {x.Latency}ms @here";
            }).ToList();
            var b = builder.WithTitle("Latency Alert")
                .WithDescription(string.Join('\n', description))
                .WithColor(embedColor)
                .Build();
        }

        private static Color DecideEmbedColorClassic(List<EndPointHealthResult> epResults)
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

        private static Color DecideEmbedColorFamily(List<EndPointHealthResult> epResults)
        {
            var average = epResults.Average(x => x.Latency);

            return average switch
            {
                > 0 and < 1000 => Color.Green,
                >= 1000 and < 2000 => Color.Orange,
                _ => Color.Red
            };
        }

        private static List<Embed> CreateClassicEmbeds(List<EndPointHealthResult> epResults)
        {
            Color embedColor = DecideEmbedColorClassic(epResults);
            EmbedBuilder builder = new EmbedBuilder();
            var b = builder.WithTitle("Latency Report")
                .WithDescription(ToClassicDescription(epResults))
                .WithColor(embedColor)
                .Build();

            return new List<Embed>() {b};
        }

        private static List<Embed> CreateFamilyEmbeds(List<EndPointHealthResult> epResults)
        {

            Dictionary<string, List<EndPointHealthResult>> split = new Dictionary<string, List<EndPointHealthResult>>();

            foreach (EndPointHealthResult epResult in epResults)
            {
                if (!split.ContainsKey(epResult.Family))
                {
                    split.Add(epResult.Family, new List<EndPointHealthResult>());
                }

                split[epResult.Family].Add(epResult);
            }

            List<Embed> embeds = split.Select(ToFamilyEmbed).ToList();
            return embeds;
        }

        private static Embed ToFamilyEmbed(KeyValuePair<string, List<EndPointHealthResult>> keyValuePair)
        {
            var average = Math.Round(keyValuePair.Value.Select(x => x.Latency).Average());
            var str =
                $"Family Report : {keyValuePair.Key} ({keyValuePair.Value.DistinctBy(x=> x.EndpointAddress).Count() } endpoints, {keyValuePair.Value.Count} runs). Average latency is {average}ms";

            EmbedBuilder builder = new EmbedBuilder();
            var slowest = keyValuePair.Value.GetSlowest();
            
            var dot = slowest.Latency switch
            {
                > 0 and < 1000 => "🟢",
                >= 1000 and < 2000 =>  "🟠",
                _ => "🔴"
            };
            
            Color embedColor = DecideEmbedColorFamily(keyValuePair.Value);
            var b = builder.WithTitle("Latency Report")
                .WithDescription(str)
                .WithColor(embedColor)
                .AddField($"{dot} Slowest Run", $"{slowest.EndpointAddress} | {slowest.Latency} ms | {slowest.DateRun}")
                .Build();

            return b;
        }

       
        
        private static string ToClassicDescription(List<EndPointHealthResult> epResults)
        {
            var str = epResults.Select(x =>
            {
                return $"{x.EndpointAddress} responds with code {x.StatusCode} in {x.Latency}ms";
            }).ToList();

            return string.Join('\n', str);
        }

       
    }
}