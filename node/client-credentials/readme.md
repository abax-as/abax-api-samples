## About this sample

This sample demonstrates usage of the Client Credentials authorization flow as described in the [documentation](https://developers.abax.cloud/openapi/getting-started#client-credentials-flow).

In this sample, a console application performs following steps:

1. Get access token using client credentials (`client_id` and `client_secret`)
2. Use the access token to list vehicles in the API. 

**Note:** By default credentials created on the developers portal only support Authorization Code Flow. Please contact ABAX if you want to use the Client Credentials Flow.

### Running the sample

Replace `clientId` and `clientSecret` in the code with your own credentials. If using Sandbox environment, also replace `apiUri` and `scopes` as described in the [documentation](https://developers.abax.cloud/openapi/getting-started#sandbox).

Run the application with
```
npm install
npm start
```

The application will print the JSON response from the API.