using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.WA.Config;
using Blazor.WA.Core;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.WA
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            var storageAccountConfig = new StorageAccountConfig();
            builder.Configuration.Bind("StorageAccount", storageAccountConfig);
            builder.Services.AddSingleton(storageAccountConfig);

            builder.Services.AddTransient<AccessTokenProviderTokenCredential>();

            builder.Services.AddTransient(sp =>
                new HttpClient
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                });

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add("https://storage.azure.com/user_impersonation");
            });

            await builder.Build().RunAsync();
        }
    }
}
