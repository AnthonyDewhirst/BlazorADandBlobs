using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace Blazor.Shared.Core
{
    public interface ICommonAccessTokenProvider
    {
        ValueTask<AccessToken> GetTokenAsync (TokenRequestContext requestContext, CancellationToken cancellationToken);

        AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken);
    }
}