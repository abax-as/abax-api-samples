using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthorizationCodeFlow.Infrastructure
{
    // This class manages the tokens. If the access token is requested, but expired, 
    // it attempts to refresh it with refresh token
    public class TokenHandler
    {
        private readonly TokenClient _client;

        // for simplicity, we store the tokens for the users in memory
        // typically, the refresh token would be stored in a backend database
        private static Dictionary<string, Tokens> _tokens = new Dictionary<string, Tokens>();

        public TokenHandler(TokenClient client)
        {
            _client = client;
        }

        public async Task RedeemAuthorizationCode(string user, string code, string redirectUri)
        {
            var tokenResponse = await _client.RedeemAuthorizationCode(code, redirectUri);
            if(tokenResponse != null)
                _tokens[user] = MapTokens(tokenResponse);                
        }
        
        public async Task<string> GetAccessToken(string user)
        {
            if (!_tokens.TryGetValue(user, out var userTokens))
                return null; // we don't have the tokens for this user

            // check if access token expired 
            if (Expired(userTokens))
            {
                // we need to refresh the access token using the refresh token
                return await RefreshAccessToken(user, userTokens);
            }

            return userTokens.AccessToken;
        }

        private async Task<string> RefreshAccessToken(string user, Tokens userTokens)
        {
            var (result, tokenResponse) = await _client.RefreshTokens(userTokens.RefreshToken);
            switch (result)
            {
                case TokenClient.RefreshResult.Ok:
                    _tokens[user] = MapTokens(tokenResponse);
                    return tokenResponse.access_token;
                case TokenClient.RefreshResult.InvalidOrExpiredRefreshToken:
                    // since we don't have a valid refresh token, reset tokens information for user
                    _tokens[user] = null;
                    return null;
                default:
                    // it was not possible to get the token, but it might have been a transient error
                    // and the refresh operation will succeed when tried later - do not remove the RT
                    return null;
            }
        }

        // use 1 minute margin for expiration
        private bool Expired(Tokens tokens) => tokens.ExpiresAt <= DateTimeOffset.Now.AddMinutes(1);

        private static Tokens MapTokens(TokenClient.TokenResponse tokenResponse) =>
            new Tokens
            {
                AccessToken = tokenResponse.access_token, 
                RefreshToken = tokenResponse.refresh_token, 
                ExpiresAt = DateTimeOffset.Now.AddSeconds(tokenResponse.expires_in)
            };

        private class Tokens
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public DateTimeOffset ExpiresAt { get; set; }
        }
    }
}