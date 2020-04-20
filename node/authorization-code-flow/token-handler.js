const tokenClient = require('./token-client');

// for simplicity, we store the tokens for the users in memory
// typically, the refresh token would be stored in a backend database
const tokens = {};

exports.redeemAuthorizationCode = async (user, code, callbackUri) => {
    const tokensResponse = await tokenClient.redeemAuthorizationCode(code, callbackUri);
    tokens[user] = mapTokens(tokensResponse);
};

exports.getAccessToken = async function(user) {
    const userTokens = tokens[user];
    if(!userTokens) 
        return null; // we don't have the tokens for this user

    if(expired(userTokens)) {
        return await refreshAccessToken(user, userTokens);
    }

    return userTokens.accessToken;
}

async function refreshAccessToken(user, userTokens) {
    const { status, response } = await tokenClient.refreshTokens(userTokens.refreshToken);

    switch (status) {
        case tokenClient.OK:
            tokens[user] = mapTokens(response);
            return response.access_token;

        case tokenClient.INVALID_OR_EXPIRED_REFRESH_TOKEN:
            // since we don't have a valid refresh token, reset tokens information for user
            tokens[user] = null;
            return null;

        case tokenClient.ERROR:
            // it was not possible to get the token, but it might have been a transient error
            // and the refresh operation will succeed when tried later - do not remove the RT
            return null;
    }
}

function expired(userTokens) {
    // use 1 minute margin for expiration
    return userTokens.expiresAt < new Date(new Date().getTime() + 60 * 1000);
}

function mapTokens(tokensResponse) {
    let now = new Date();
    return {
        accessToken: tokensResponse.access_token,
        refreshToken: tokensResponse.refresh_token,
        expiresAt: new Date(now.getTime() + tokensResponse.expires_in * 1000)
    };
}
