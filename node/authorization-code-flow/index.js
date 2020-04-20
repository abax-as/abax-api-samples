const app = require('express')();
const mustache = require('mustache-express');
const path = require('path');
const tokenHandler = require('./token-handler');
const config = require('./config');
const axios = require('axios');

const PORT = process.env.PORT = 4000;

// this represents your user currently logged in into your system
// normally, you would get the user from your authentication system
// here, for simplicity, we assume that logged in user is "user1"
const USER = "user1";

// setup mustache engine
app.engine('html', mustache());
app.set('view engine', 'html');
app.set('views', path.join(__dirname, 'views'));

app.get('/', async (req, res) => {
    const accessToken = await tokenHandler.getAccessToken(USER);
    if(!accessToken) {
        // if we don't have the token yet, redirect to authentication handler
        res.redirect('/auth')
    } else {
        const vehicles = await callApi(accessToken);
        res.render('index.html', { data: JSON.stringify(vehicles) });
    }
});

app.get('/auth', (req, res) => {
    // replace with sanbox scopes if working with Sandbox environment
    const scopes = "openid abax_profile open_api open_api.vehicles offline_access";
    const redirectUri = getCallbackUri();

    const uri = `${config.identityProviderUri}/connect/authorize?response_type=code&scope=${encodeURIComponent(scopes)}&client_id=${config.clientId}&redirect_uri=${encodeURIComponent(redirectUri)}`;

    res.redirect(uri);
});

app.get('/auth/callback', async (req, res) => {
    await tokenHandler.redeemAuthorizationCode(USER, req.query.code, getCallbackUri());
    // now that we have token, we're ready to get back to calling the API
    res.redirect('/');
});

function getCallbackUri() {
    return `http://localhost:${PORT}/auth/callback`;
}

async function callApi(token) {
    console.info('Calling the API...');
    
    // add authorization header to the request
    const headers = { headers: { 'Authorization': `Bearer ${token}` }};

    try {
        // call vehicles endpoint
        const response = await axios.get(`${config.apiUri}/v1/vehicles`, headers);
        console.info('API call successful');
        return response.data;

    } catch (e) {
        console.error('API request failed: ', e.message);
        return null;
    }
}

app.listen(PORT, () => {
    console.info(`Listening on http://localhost:${PORT}`);
});
