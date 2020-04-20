const axios = require('axios');
const config = require('./config');
const qs = require('querystring');

exports.OK = 'OK';
exports.INVALID_OR_EXPIRED_REFRESH_TOKEN = 'INVALID_OR_EXPIRED_REFRESH_TOKEN';
exports.ERROR = 'ERROR';

exports.redeemAuthorizationCode = async (code, callbackUri) => {
    console.info("Getting the tokens from identity provider...");    
            
    // here we demonstrate getting the token without any external libraries
    // you may however consider using https://www.npmjs.com/package/openid-client

    const content = {
        'grant_type': 'authorization_code',
        'code': code,
        'client_id': config.clientId,
        'client_secret': config.clientSecret,
        'redirect_uri': callbackUri
    };

    const headers = { headers: { 'Content-Type': 'application/x-www-form-urlencoded' }};

    try {
        var response = await axios.post(
            `${config.identityProviderUri}/connect/token`,
            qs.stringify(content),
            headers);

        console.info('Token request successful');
        // when token expires, you need to get a new one
        console.info(`Access token expires in ${response.data.expires_in} seconds`);
        return response.data;

    } catch (e) {
        console.error('Token request failed: ', e.message);
        if(e.response && e.response.status === 400)
            console.error(e.response.data);
        return null;
    }
};

exports.refreshTokens = async (refreshToken) => {
    console.info("Refreshing the access token...");    
            
    // here we demonstrate getting the token without any external libraries
    // you may however consider using https://www.npmjs.com/package/openid-client

    const content = {
        'grant_type': 'refresh_token',
        'refresh_token': refreshToken,
        'client_id': config.clientId,
        'client_secret': config.clientSecret
    };

    const headers = { headers: { 'Content-Type': 'application/x-www-form-urlencoded' }};

    try {
        var response = await axios.post(
            `${config.identityProviderUri}/connect/token`,
            qs.stringify(content),
            headers);

        console.info('Token refresh successful');
        // when token expires, you need to get a new one
        console.info(`Access token expires in ${response.data.expires_in} seconds`);
        return { status: exports.OK, response: response.data };

    } catch (e) {
        console.error('Token request failed: ', e.message);
        if(e.response && e.response.status === 400) {
            // check specifically for error code that notifies about refresh token being invalid or expired
            if(e.response.data.error === 'invalid_grant') {
                console.warn('Refresh token is invalid or expired');
                return { status: exports.INVALID_OR_EXPIRED_REFRESH_TOKEN, response: null }
            } else {
                console.error(e.response.data);
            }
        }
        return { status: exports.ERROR, response: null };
    }
};