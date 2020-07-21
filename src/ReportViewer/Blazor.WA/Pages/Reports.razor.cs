using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure.Core;
using Azure.Storage.Blobs;
using Blazor.WA.Config;
using Blazor.WA.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Blazor.WA.Pages
{
    public class ReportsBase : ComponentBase
    {
        protected List<string> Reports;

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; }

        [Inject]
        private AccessTokenProviderTokenCredential Credential { get; set; }

        [Inject]
        private IAccessTokenProvider AccessTokenProvider { get; set; }

        [Inject]
        private StorageAccountConfig StorageAccountConfig { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Reports = new List<string>();

            // these both work
            await LoadViaClient();
            //await LoadUsingApiDirect();
        }

        private async Task LoadViaClient()
        {
            var client = new BlobContainerClient(StorageAccountConfig.Uri, Credential);

            await foreach (var blobItem in client.GetBlobsAsync(prefix: StorageAccountConfig.FolderPrefix))
            {
                Reports.Add(blobItem.Name);
            }
        }

        private async Task LoadUsingApiDirect()
        {
            var uri = $"{StorageAccountConfig.Uri}?restype=container&comp=list&prefix={StorageAccountConfig.FolderPrefix}";
            
            // Instantiate the request message with a null payload.
            var now = DateTime.UtcNow;
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
            httpRequestMessage.Headers.Add("x-ms-version", "2019-12-12");

            var headerValue = "Bearer " + await GetTokenAsync();
            httpRequestMessage.Headers.Add(HttpHeader.Names.Authorization, headerValue);

            // Send the request.
            using var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage, CancellationToken.None);

            // If successful (status code = 200),
            //   parse the XML response for the container names.
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var xmlString = await httpResponseMessage.Content.ReadAsStringAsync();
                var x = XElement.Parse(xmlString);
                foreach (var container in x.Element("Containers").Elements("Container"))
                {
                    Reports.Add(container.Element("Name").Value);
                }
            }
        }

        public async Task<string> GetTokenAsync()
        {
            var result = await AccessTokenProvider.RequestAccessToken();

            if (result.Status == AccessTokenResultStatus.RequiresRedirect)
            {
                //_navigationManager.NavigateTo(result.RedirectUrl);
            }

            if (result.TryGetToken(out var accessToken))
            {
                return accessToken.Value;
            }
            throw new Exception("Couldn't get the access token");
        }

        protected void ViewReport(string reportName)
        {
            var uri = QueryHelpers.AddQueryString("report", new Dictionary<string, string> {{"reportName", reportName}});

            NavigationManager.NavigateTo(uri);
        }
    }
}