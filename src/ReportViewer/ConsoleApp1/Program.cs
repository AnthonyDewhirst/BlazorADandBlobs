using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.development.json", optional:true);
            var config = builder.Build();

            var adConfig = new AzureAd();
            config.Bind("AzureAd", adConfig);

            var storageConfig = new StorageAccountConfig();
            config.Bind("StorageAccount", storageConfig);

            var scopes = new[] { "https://storage.azure.com/user_impersonation" };
            var app = PublicClientApplicationBuilder
                .Create(adConfig.ClientId)
                .WithAuthority(adConfig.Authority)
                .WithRedirectUri("http://localhost")
                .Build();
            var accounts = await app.GetAccountsAsync();
            AuthenticationResult result;
            try
            {
                result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await app.AcquireTokenInteractive(scopes)
                    .ExecuteAsync();
            }

            var credential = new AccessTokenProviderTokenCredential(result.AccessToken, result.ExpiresOn.DateTime);
            var client = new BlobContainerClient(storageConfig.Uri, credential);

            var reports = new List<string>();

            await foreach (var blobItem in client.GetBlobsAsync(prefix: storageConfig.FolderPrefix))
            {
                reports.Add(blobItem.Name);
            }

            var reportName = reports.First();

            for (var i = 0; i < 3; i++)
            {
                var blobClient = client.GetBlockBlobClient(reportName);

                var blob = await blobClient.DownloadAsync();

                Console.WriteLine("Status Code:" + blob.GetRawResponse().Status);
            }
        }
    }
}
