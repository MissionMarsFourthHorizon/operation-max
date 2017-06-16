/* jshint esversion: 6 */
const restify = require('restify');

module.exports = (config) => {
    return (query, callback) => {
        const client = restify.createJsonClient({
            url: `https://westus.api.cognitive.microsoft.com`,
            headers: {
                'Ocp-Apim-Subscription-Key': config.apiKey
            }
        });

        const payload = {
            documents: [{
                language: 'en',
                id: 'singleId',
                text: query
            }]
        };

        const urlPath = '/text/analytics/v2.0/sentiment';

        client.post(urlPath, payload, (err, request, response, result) => {
            if (!err &&
                response &&
                response.statusCode == 200 &&
                result.documents[0]) {
                callback(null, result.documents[0].score);
            } else {
                callback(err, null);
            }
        });
    };
};

