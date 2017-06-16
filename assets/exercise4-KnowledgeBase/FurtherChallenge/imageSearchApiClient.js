/* jshint esversion: 6 */
const restify = require('restify');

module.exports = (config) => {
    return (query, callback) => {
        const client = restify.createJsonClient({
            url: 'https://api.cognitive.microsoft.com',
            headers: {
                'Ocp-Apim-Subscription-Key': config.apiKey
            }
        });

        const urlPath = `/bing/v5.0/images/search?q=${query}&count=10&safeSearch=Strict&imageType=ClipArt`;

        var index = Math.floor(Math.random() * 9);

        client.post(urlPath, null, (err, request, response, result) => {
            if (!err &&
                response &&
                response.statusCode == 200 &&
                result.value[0]) {
                callback(null, result.value[index].thumbnailUrl);
            } else {
                callback(err, null);
            }
        });
    };
};

