/* jshint esversion: 6 */
const restify = require('restify');

module.exports = (config) => {
    return (query, callback) => {
        const client = restify.createJsonClient({ url: `https://${config.searchName}.search.windows.net/` });
        var urlPath = `/indexes/${config.indexName}/docs?api-key=${config.searchKey}&api-version=2016-09-01&${query}`;

        client.get(urlPath, (err, request, response, result) => {
            if (!err && response && response.statusCode == 200) {
                callback(null, result);
            } else {
                callback(err, null);
            }
        });
    };
};
