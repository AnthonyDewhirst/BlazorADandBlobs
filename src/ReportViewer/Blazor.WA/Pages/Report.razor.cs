using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Blazor.WA.Config;
using Blazor.WA.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Blazor.WA.Pages
{
    public class ReportBase : ComponentBase
    {
        protected BlobDownloadInfo Blob;

        [Inject]
        private AccessTokenProviderTokenCredential Credential { get; set; }

        [Inject]
        private StorageAccountConfig StorageAccountConfig { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var client = new BlobContainerClient(StorageAccountConfig.Uri, Credential);

            var query = new Uri(NavigationManager.Uri).Query;
            var reportName = "";

            if (QueryHelpers.ParseQuery(query).TryGetValue("reportName", out var value))
            {
                reportName = value;
            }

            var blobClient = client.GetBlockBlobClient(reportName);

            var blob = await blobClient.DownloadAsync();

            Blob = blob;
        }
    }
}