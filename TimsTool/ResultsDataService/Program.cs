using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.Extensions.Configuration;
using System;

namespace ResultsDataService
{
    public class Program
    {
        private static Uri kvUri = new Uri("https://resultstreevault.vault.azure.net/");
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var secretClient = new SecretClient(kvUri, new DefaultAzureCredential());
                config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
            })
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}