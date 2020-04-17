using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AuthorizationCodeFlow.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthorizationCodeFlow.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly TokenHandler _tokenHandler;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(
            ILogger<IndexModel> logger, 
            TokenHandler tokenHandler, 
            IHttpClientFactory clientFactory, 
            IConfiguration configuration)
        {
            _logger = logger;
            _tokenHandler = tokenHandler;
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGet()
        {
            var accessToken = await _tokenHandler.GetAccessToken(UserContext.User);
            if (string.IsNullOrEmpty(accessToken))
                // we don't have an access token yet - go to auth handler
                return RedirectToPage("AuthHandler");

            // we're ready to call the API
            ViewData["vehicles"] = await GetVehicles(accessToken);
            return Page();
        }

        private async Task<string> GetVehicles(string token)
        {
            _logger.LogInformation("Calling the API...");

            // call vehicles endpoint 
            var request = new HttpRequestMessage(HttpMethod.Get, 
                new Uri(new Uri(_configuration["Api:Url"]), "v1/vehicles"));
            // add authorization header to the request
            // Authorization: Bearer token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            
            _logger.Log(response.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Error,
                $"API call resulted in HTTP {(int) response.StatusCode} code");
            return await response.Content.ReadAsStringAsync();
        }
    }
}