using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Blazor.Shared.Config;
using Blazor.Shared.Core;
using Blazor.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Blazor.Shared
{
    public class ListReportsBase : ComponentBase
    {
        protected List<string> Blobs;

        [Inject]
        private AccessTokenProviderTokenCredential Credential { get; set; }

        [Inject] 
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private StorageAccountConfig StorageAccountConfig { get; set; }

        [Inject]
        private SimpleSessionState SimpleSessionState { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var client = new BlobContainerClient(StorageAccountConfig.Uri, Credential);

            Blobs = new List<string>();

            await foreach (var blobItem in client.GetBlobsAsync(prefix: StorageAccountConfig.FolderPrefix))
            {
                Blobs.Add(blobItem.Name);
            }
        }
        
        protected void ViewReport(string blob)
        {
            NavigationManager.NavigateTo("report");
            SimpleSessionState.SelectedReport = blob;
        }
    }
}