## About this sample

This sample demonstrates usage of the Authorization Code flow flow as described in the [documentation](https://developers.abax.cloud/openapi/getting-started#authorization-code-flow).

The demonstrated use case shows how to get access and refresh token for an ABAX user. These tokens can then be used to perform operations on the API, even without user interaction (e.g. by a background task). We assume that you have your own authentication system with your own users and you don't want to integrate that with ABAX Identity Provider.

If you are looking for an example on how to use ABAX Identity Provider as an authentication provider (similar to Google, Facebook or Microsoft account), please refer to [Microsoft architecture guide](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/secure-net-microservices-web-applications/#authenticate-with-an-openid-connect-or-oauth-20-identity-provider) or any other example on using OpenID Connect providers.


In this sample, a web application performs following steps:

1. Redirect the user to ABAX Identity Provider for authentication and consent.
2. Upon return from the Identity Provider, run a backchannel request to exchange the received authorization code for access and refresh token.
3. Use the access token to list vehicles in the API. 
4. If access token expires, use refresh token to get a new one.

### Points of interest

* `appsettings.json` contains configuration of the API credentials
* `Index.cshtml.cs` page attempts to display API response (or initiate authorization flow if token is not yet available)
* `AuthHandler.cshtml.cs` handles the web part of the flow
* `TokenHandler` and `TokenClient` are responsible for backchannel communication and token refreshing

### Alternatives

In this sample we demonstrate how to work with OpenID Connect protocol directly, without using any external libraries. You may also want to take a look at [IdentityModel.OidcClient](`https://github.com/IdentityModel/IdentityModel.OidcClient`) library and their [samples](https://github.com/IdentityModel/IdentityModel.OidcClient.Samples/tree/master/NetCoreConsoleClient).


### Running the sample

Replace `ClientId` and `ClientSecret` in the `appsettings.json` file with your own credentials. The credentials need to have a proper redirect URI assigned on the developers portal. Add `https://localhost:5001/AuthHandler` as the redirect URI. 

If using Sandbox environment:
* replace `ApiUrl` in the settings
* replace scopes used by `TokenClient` as described in the [documentation](https://developers.abax.cloud/openapi/getting-started#sandbox).


Run the application with
```
dotnet run --project AuthorizationCodeFlow/AuthorizationCodeFlow.csproj
```

Visit [https://localhost:5001](https://localhost:5001). After successful authentication, the application will display JSON response from the API on the page.

**Note:** The application makes use of .NET developer certificates for HTTPS. You can ensure they are trusted by issuing following command:
```
dotnet dev-certs https --trust
```