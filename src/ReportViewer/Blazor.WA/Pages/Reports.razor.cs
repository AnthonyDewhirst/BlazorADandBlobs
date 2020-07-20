using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Blazor.WA.Config;
using Blazor.WA.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Blazor.WA.Pages
{
    public class ReportsBase : ComponentBase
    {
        protected List<string> Reports;

        [Inject]
        private AccessTokenProviderTokenCredential Credential { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private StorageAccountConfig StorageAccountConfig { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var client = new BlobContainerClient(StorageAccountConfig.Uri, Credential);

            Reports = new List<string>();

            await foreach (var blobItem in client.GetBlobsAsync(prefix: StorageAccountConfig.FolderPrefix))
            {
                Reports.Add(blobItem.Name);
            }
        }

        protected void ViewReport(string reportName)
        {
            var uri = QueryHelpers.AddQueryString("report", new Dictionary<string, string> {{"reportName", reportName}});

            NavigationManager.NavigateTo(uri);
        }
    }
}