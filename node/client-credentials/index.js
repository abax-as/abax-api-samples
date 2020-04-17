const axios = require('axios');
const qs = require('querystring');

// client id and client secret generated on the ABAX Developer Portal
// NOTE: by default the credentials will only support Authorization Code Flow,
// contact us to switch to Client Credentials Flow
// NOTE: these settings need to be kept confidential in your application, because
// they enable access to all data of the organization
const clientId = "[client_id]";
const clientSecret = "[client_seret]";

// scopes of data that will be possible to access with the token
// see documentation at ABAX Developer Portal for more information
// NOTE: use sandbox scopes when working with the Sandbox API,
// e.g. "open_api.sandbox open_api.sandbox.vehicles
const scopes = "open_api open_api.vehicles";

const identityProviderUri =  "https://identity.abax.cloud";

// use sandbox url when working with Sandbox API (https://api-test.abax.cloud)
const apiUri = "https://api.abax.cloud";

(async function () {
    const token = await getAccessToken();
    if(token !== null)
        await callApi(token);
})();

async function getAccessToken() {
    console.info('Requesting token...');

    const content = {
        'grant_type': 'client_credentials',
        'scope': scopes,
        'client_id': clientId,
        'client_secret': clientSecret
    };

    const config = {
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        }
    };

    try {
        var response = await axios.post(
            `${identityProviderUri}/connect/token`,
            qs.stringify(content),
            config);

        console.info('Token request successful');
        // when token expires, you need to get a new one
        console.info(`Token expires in ${response.data.expires_in} seconds`);
        return response.data.access_token;

    } catch (e) {
        console.error('Token request failed: ', e.message);
        if(e.response && e.response.status === 400)
            console.error(e.response.data);
        return null;
    }
}

async function callApi(token) {
    console.info('Calling the API...');
    
    // add authorization header to the request
    const config = { 
        headers: {
            'Authorization': `Bearer ${token}`
        }
    };

    try {
        // call vehicles endpoint
        var response = await axios.get(`${apiUri}/v1/vehicles`, config);
        console.info('API call successful');
        console.info(response.data);

    } catch (e) {
        console.error('API request failed: ', e.message);
    }
}