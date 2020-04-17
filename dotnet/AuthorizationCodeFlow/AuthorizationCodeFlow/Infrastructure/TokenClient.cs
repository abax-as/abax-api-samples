using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthorizationCodeFlow.Infrastructure
{
    public class TokenClient
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<TokenClient> _logger;

        public TokenClient(
            IConfiguration configuration, 
            IHttpClientFactory clientFactory, 
            ILogger<TokenClient> logger)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<TokenResponse> RedeemAuthorizationCode(string code, string callbackUri)
        {
            _logger.LogInformation("Getting the tokens from identity provider...");    
            
            // here we demonstrate getting the token without any external libraries
            // you may however consider using https://github.com/IdentityModel/IdentityModel.OidcClient
            
            var request = new HttpRequestMessage(HttpMethod.Post,
                new Uri(new Uri(_configuration["Api:IdentityProvider"]), "connect/token"))
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["client_id"] = _configuration["Api:ClientId"],
                    ["client_secret"] = _configuration["Api:ClientSecret"],
                    ["redirect_uri"] = callbackUri // use exactly the same uri as when redirecting the browser to login page
                })
            };

            using var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Token request successful");
                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

                // when token expires, you need to refresh it
                _logger.LogInformation($"Access token expires in {tokenResponse.expires_in} seconds");
                return tokenResponse;
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token request failed");
                _logger.LogError(result);

                return null;
            }
        }

        public async Task<(RefreshResult, TokenResponse)> RefreshTokens(string refreshToken)
        {
            _logger.LogInformation("Refreshing the access token...");    
            
            // here we demonstrate getting the token without any external libraries
            // you may however consider using https://github.com/IdentityModel/IdentityModel.OidcClient
            
            var request = new HttpRequestMessage(HttpMethod.Post,
                new Uri(new Uri(_configuration["Api:IdentityProvider"]), "connect/token"))
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken,
                    ["client_id"] = _configuration["Api:ClientId"],
                    ["client_secret"] = _configuration["Api:ClientSecret"]
                })
            };

            using var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Token refresh successful");
                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

                // when token expires, you need to refresh it
                _logger.LogInformation($"Access token expires in {tokenResponse.expires_in} seconds");
                return (RefreshResult.Ok, tokenResponse);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token refresh failed");
                _logger.LogError(result);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorResponse = JsonSerializer.Deserialize<TokenError>(result);
                    if (errorResponse.error == "invalid_grant")
                    {
                        // we explicitly handle the case of invalid or expired refresh token
                        return (RefreshResult.InvalidOrExpiredRefreshToken, null);
                    }
                }

                throw new Exception("Token request failed");
            }
        }

        public enum RefreshResult
        {
            Ok,
            InvalidOrExpiredRefreshToken
        }

        public class TokenResponse
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
        }

        private class TokenError
        {
            public string error { get; set; }
        } 
    }
}