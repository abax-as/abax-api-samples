using System;
using System.Net;
using System.Threading.Tasks;
using AuthorizationCodeFlow.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthorizationCodeFlow.Pages
{
    public class AuthHandler : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly TokenHandler _tokenHandler;
        private readonly ILogger<AuthHandler> _logger;

        public AuthHandler(
            IConfiguration configuration,
            TokenHandler tokenHandler,
            ILogger<AuthHandler> logger)
        {
            _configuration = configuration;
            _tokenHandler = tokenHandler;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            if (string.IsNullOrEmpty(Request.Query["code"]))
            {
                // no code in query string, this means we're initiating the authentication
                _logger.LogInformation("Redirecting to identity provider");
                return RedirectToIdentityProvider();
            }
            else
            {
                _logger.LogInformation("Received authorization code");

                // get and store the tokens using the received code
                await _tokenHandler.RedeemAuthorizationCode(
                    UserContext.User,Request.Query["code"].ToString(), GetCallbackUri());
                
                // now we're ready to call the API
                return RedirectToPage("Index");
            }
        }

        private IActionResult RedirectToIdentityProvider()
        {
            // request access to vehicles API as a user
            // for more information on the scopes, please refer to documentation at ABAX Developer Portal
            var scopes = "openid abax_profile open_api open_api.vehicles offline_access";

            var clientId = _configuration["Api:ClientId"];

            // this is the URI to which identity provider will redirect after authentication
            // it needs to match the redirect_uri configured on the credentials in ABAX Developer Portal
            // here we want to come back to the AuthHandler page
            var redirectUri = GetCallbackUri();

            var uri = $"{_configuration["Api:IdentityProvider"]}/connect/authorize?response_type=code&scope={WebUtility.UrlEncode(scopes)}&client_id={clientId}&redirect_uri={WebUtility.UrlEncode(redirectUri)}";

            return Redirect(uri);
        }


        private string GetCallbackUri()
        {
            // absolute url to current page, with empty query string
            var builder = new UriBuilder();
            builder.Scheme = Request.Scheme;
            builder.Host = Request.Host.Host;
            if (Request.Host.Port != null)
                builder.Port = Request.Host.Port.Value;
            builder.Path = Request.Path;

            return builder.ToString();
        }
    }
}