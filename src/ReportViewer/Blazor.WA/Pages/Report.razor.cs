using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Blazor.WA.Config;
using Blazor.WA.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Blazor.WA.Pages
{
    public class ReportBase : ComponentBase
    {
        protected BlobDownloadInfo Blob;
        protected string Json;

        [Inject]
        private AccessTokenProviderTokenCredential Credential { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; }

        [Inject]
        private StorageAccountConfig StorageAccountConfig { get; set; }

        [Inject]
        private IAccessTokenProvider AccessTokenProvider { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private ILogger<ReportBase> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var query = new Uri(NavigationManager.Uri).Query;
            var reportName = "";

            if (QueryHelpers.ParseQuery(query).TryGetValue("reportName", out var value))
            {
                reportName = value;
            }

            // This doesn't work:
            //await LoadViaClient(reportName);

            // This does work:
            await LoadUsingApiDirect(reportName);
        }

        private async Task LoadViaClient(string reportName)
        {
            var client = new BlobContainerClient(StorageAccountConfig.Uri, Credential);
            var blobClient = client.GetBlockBlobClient(reportName);

            var blob = await blobClient.DownloadAsync();

            Blob = blob;
        }


        private async Task LoadUsingApiDirect(string reportName)
        {
            var uri = $"{StorageAccountConfig.Uri}/{reportName}";

            // Instantiate the request message with a null payload.
            var now = DateTime.UtcNow;
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
            httpRequestMessage.Headers.Add("x-ms-version", "2019-12-12");

            var sendRequestId = Guid.NewGuid().ToString();

            httpRequestMessage.Headers.Add("x-ms-client-request-id", sendRequestId);
            Logger.LogInformation($"Sent request id was: {sendRequestId}");

            var headerValue = "Bearer " + await GetTokenAsync();
            httpRequestMessage.Headers.Add(HttpHeader.Names.Authorization, headerValue);

            // Send the request.
            using var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage, CancellationToken.None);

            // If successful (status code = 200),
            //   parse the XML response for the container names.
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                Logger.LogInformation($"Returned request id was: {httpResponseMessage.Headers.GetValues("x-ms-client-request-id").FirstOrDefault()}");

                Json = await httpResponseMessage.Content.ReadAsStringAsync();

                Logger.LogInformation($"json loaded: {Json.Length}");
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
    }
}