using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Blazor.Shared.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using AccessToken = Azure.Core.AccessToken;

namespace Blazor.WebAssemblyShared.Core
{
    public class WebAssemblyAccessTokenProvider : ICommonAccessTokenProvider
    {
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly NavigationManager _navigationManager;

        public WebAssemblyAccessTokenProvider(IAccessTokenProvider accessTokenProvider, NavigationManager navigationManager)
        {
            _accessTokenProvider = accessTokenProvider;
            _navigationManager = navigationManager;
        }

        public async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var result = await _accessTokenProvider.RequestAccessToken(new AccessTokenRequestOptions
            {
                Scopes = new [] { "https://storage.azure.com/user_impersonation" }
            });

            if (result.Status == AccessTokenResultStatus.RequiresRedirect)
            {
                _navigationManager.NavigateTo(result.RedirectUrl);
            }

            if (result.TryGetToken(out var accessToken))
            {
                return new AccessToken(accessToken.Value, accessToken.Expires);
            }
            throw new Exception("Couldn't get the access token");
        }

        public AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}