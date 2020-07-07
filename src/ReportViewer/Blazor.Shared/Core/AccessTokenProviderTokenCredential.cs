using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace Blazor.Shared.Core
{
    public class AccessTokenProviderTokenCredential : TokenCredential
    {
        private readonly ICommonAccessTokenProvider _accessTokenProvider;

        public AccessTokenProviderTokenCredential(ICommonAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return _accessTokenProvider.GetTokenAsync(requestContext, cancellationToken);
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return _accessTokenProvider.GetToken(requestContext, cancellationToken);
        }
    }
}