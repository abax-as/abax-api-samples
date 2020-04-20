## About this sample

This sample demonstrates usage of the Authorization Code flow flow as described in the [documentation](https://developers.abax.cloud/openapi/getting-started#authorization-code-flow).

The demonstrated use case shows how to get access and refresh token for an ABAX user. These tokens can then be used to perform operations on the API, even without user interaction (e.g. by a background task). We assume that you have your own authentication system with your own users and you don't want to integrate that with ABAX Identity Provider.

If you are looking for an example on how to use ABAX Identity Provider as an authentication provider (similar to Google, Facebook or Microsoft account), please refer to [Passport documentation](http://www.passportjs.org/docs/openid/) or any other example on using OpenID Connect providers.


In this sample, a web application performs following steps:

1. Redirect the user to ABAX Identity Provider for authentication and consent.
2. Upon return from the Identity Provider, run a backchannel request to exchange the received authorization code for access and refresh token.
3. Use the access token to list vehicles in the API. 
4. If access token expires, use refresh token to get a new one.

### Points of interest

* `config.js` contains configuration of the API credentials
* `/` handler attempts to display API response (or initiate authorization flow if token is not yet available)
* `/auth` and `/auth/callback` handle the web part of the flow
* `tokenHandler.js` and `tokenClient.js` are responsible for backchannel communication and token refreshing

### Alternatives

In this sample we demonstrate how to work with OpenID Connect protocol directly, without using any external libraries. You may also want to take a look at [openid-client](https://www.npmjs.com/package/openid-client) library that implements the backchannel communication.

### Running the sample

Replace `clientId` and `clientSecret` in the `config.js` file with your own credentials. The credentials need to have a proper redirect URI assigned on the developers portal. Add `http://localhost:4000/auth/callback` as the redirect URI. 

If using Sandbox environment:
* replace `apiUri` in the settings
* replace scopes used by `/auth` handler as described in the [documentation](https://developers.abax.cloud/openapi/getting-started#sandbox).


Run the application with
```
npm start
```

Visit [http://localhost:4000](https://localhost:4000). After successful authentication, the application will display JSON response from the API on the page.
