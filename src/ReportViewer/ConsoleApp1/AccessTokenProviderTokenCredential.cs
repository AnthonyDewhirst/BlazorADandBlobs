using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace ConsoleApp1
{
    public class AccessTokenProviderTokenCredential : TokenCredential
    {
        private readonly string _accessToken;
        private readonly DateTime _expires;

        public AccessTokenProviderTokenCredential(string accessToken, DateTime expires)
        {
            _accessToken = accessToken;
            _expires = expires;
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
                return new ValueTask<AccessToken>(Task.FromResult(new AccessToken(_accessToken, _expires)));
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}