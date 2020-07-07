using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Blazor.Shared.Config;
using Blazor.Shared.Core;
using Blazor.Shared.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Blazor.Shared
{
    public class ViewReportBase : ComponentBase
    {
        protected BlobDownloadInfo Blob;

        [Inject]
        private SimpleSessionState SimpleSessionState { get; set; }

        [Inject]
        private AccessTokenProviderTokenCredential Credential { get; set; }

        [Inject]
        private StorageAccountConfig StorageAccountConfig { get; set; }

        [Inject]
        private ILogger<ViewReportBase> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var client = new BlobContainerClient(StorageAccountConfig.Uri, Credential);

            Logger.LogInformation($"Loading report: [{SimpleSessionState.SelectedReport}]");

            var blobClient = client.GetBlockBlobClient(SimpleSessionState.SelectedReport);

            var blob = await blobClient.DownloadAsync();

            Logger.LogInformation($"Loaded report: [{SimpleSessionState.SelectedReport}]");

            Blob = blob;
        }
    }
}