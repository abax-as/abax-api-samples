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
                return await RefreshAccessToken(user);
            }

            return userTokens.AccessToken;
        }

        private async Task<string> RefreshAccessToken(string user)
        {
            var (result, tokenResponse) = await _client.RefreshTokens(_tokens[user].RefreshToken);
            if (result == TokenClient.RefreshResult.Ok)
                _tokens[user] = MapTokens(tokenResponse);
            else
                // the refresh token is invalid or expired, the user needs to authenticate again
                _tokens[user] = null;

            return tokenResponse?.access_token;
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