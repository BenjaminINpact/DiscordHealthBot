using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordHealBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            var configuration = builder.Build();

            
            var serviceProvider = new ServiceCollection()
                .AddLogging(conf => conf.AddConsole());
            
            Startup startup = new Startup(configuration, serviceProvider);
        
            await startup.RunAsync();


        }
    }
}