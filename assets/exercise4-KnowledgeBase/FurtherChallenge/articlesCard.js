/* jshint esversion: 6 */
const builder = require('botbuilder');

module.exports = (item, query) => {

    let card = {
        contentType: 'application/vnd.microsoft.card.adaptive',
        content: {
            type: 'AdaptiveCard',
            body: [
                {
                    type: 'ColumnSet',
                    columns: [
                        {
                            type: 'Column',
                            size: '1',
                            items: [
                                {
                                    type: 'TextBlock',
                                    size: 'large',
                                    weight: 'bolder',
                                    text: item.title,
                                    wrap: true
                                },
                                {
                                    type: 'FactSet',
                                    separation: 'none',
                                    facts: [
                                        {
                                            title: 'Search Score:',
                                            value: item['@search.score'].toString()
                                        },
                                        {
                                            title: 'Category:',
                                            value: item.category
                                        }
                                    ]
                                }
                            ]
                        },
                        {
                            type: 'Column',
                            size: 'auto',
                            items: [
                                {
                                    type: 'Image',
                                    url: 'https://bot-framework.azureedge.net/bot-icons-v1/bot-framework-default-7.png',
                                    size: 'medium'
                                }
                            ]
                        }
                    ]
                },
                {
                    type: 'TextBlock',
                    text: item.text,
                    maxLines: '2',
                    wrap: true,
                    size: 'normal'
                }
            ],
            actions: [
                {
                    type: 'Action.Submit',
                    title: 'More Details',
                    data: `show me the article ${item.title}`
                }
            ]
        }
    };

    return card;
};
