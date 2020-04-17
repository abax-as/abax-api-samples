using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClientCredentials
{
    class Program
    {
        // client id and client secret generated on the ABAX Developer Portal
        // NOTE: by default the credentials will only support Authorization Code Flow,
        // contact us to switch to Client Credentials Flow
        // NOTE: these settings need to be kept confidential in your application, because
        // they enable access to all data of the organization
        private const string ClientId = "client id";
        private const string ClientSecret = "client secret";

        // scopes of data that will be possible to access with the token
        // see documentation at ABAX Developer Portal for more information
        // NOTE: use sandbox scopes when working with the Sandbox API,
        // e.g. "open_api.sandbox open_api.sandbox.vehicles
        private const string Scopes = "open_api open_api.vehicles";

        private static readonly Uri IdentityProviderUri = new Uri("https://identity.abax.cloud");

        // use sandbox url when working with Sandbox API (https://api-test.abax.cloud)
        private static readonly Uri ApiUri = new Uri("https://api.abax.cloud");

        static async Task Main(string[] args)
        {
            var token = await GetAccessToken();
            await CallApi(token);
        }

        private static async Task<string> GetAccessToken()
        {
            // this example shows how to get the token without using any external libraries
            // you may however consider using https://github.com/IdentityModel/IdentityModel
            
            Console.WriteLine("Requesting token...");

            // send a request for token to the token endpoint
            // the request needs to be form url encoded
            var request = new HttpRequestMessage(HttpMethod.Post,
                new Uri(IdentityProviderUri, "connect/token"))
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["scope"] = Scopes,
                    ["client_id"] = ClientId,
                    ["client_secret"] = ClientSecret
                })
            };

            // in production code, reuse the HttpClient to avoid port exhaustion
            using var client = new HttpClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Token request successful");
                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

                // when token expires, you need to get a new one
                Console.WriteLine($"Token expires in {tokenResponse.expires_in} seconds");
                return tokenResponse.access_token;
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Token request failed:");
                Console.WriteLine(result);

                throw new Exception("Token request failed");
            }
        }

        private static async Task CallApi(string token)
        {
            Console.WriteLine("Calling the API...");

            // call vehicles endpoint 
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(ApiUri, "v1/vehicles"));
            // add authorization header to the request
            // Authorization: Bearer token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var client = new HttpClient();
            var response = await client.SendAsync(request);

            Console.WriteLine($"API call resulted in HTTP {(int) response.StatusCode} code");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }


        class TokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
            public int expires_in { get; set; }
        }
    }
}