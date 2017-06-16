namespace HelpDesk.Util
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HelpDeskBot.Model;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using AdaptiveCards;

    public static class CardUtil
    {
        public static async Task ShowSearchResults(IDialogContext context, SearchResult searchResult, string notResultsMessage)
        {
            Activity reply = ((Activity)context.Activity).CreateReply();
            
            await CardUtil.ShowSearchResults(reply, searchResult, notResultsMessage);
        }

        public static async Task ShowSearchResults(Activity reply, SearchResult searchResult, string notResultsMessage)
        {
            if (searchResult.Value.Length != 0)
            {
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                foreach (SearchResultHit item in searchResult.Value)
                {
                    reply.Attachments.Add(
                        new Attachment
                        {
                            ContentType = "application/vnd.microsoft.card.adaptive",
                            Content = CreateSearchResultCard(item.Title, item.Category, item.SearchScore.ToString(), item.Text)
                        }
                    );
                }

                ConnectorClient connector = new ConnectorClient(new Uri(reply.ServiceUrl));
                await connector.Conversations.SendToConversationAsync(reply);
            }
            else
            {
                reply.Text = notResultsMessage;
                ConnectorClient connector = new ConnectorClient(new Uri(reply.ServiceUrl));
                await connector.Conversations.SendToConversationAsync(reply);
            }            
        }

        private static AdaptiveCard CreateSearchResultCard(string title, string category, string score, string text)
        {
            AdaptiveCard card = new AdaptiveCard();
            
            var columnsBlock = new ColumnSet()
            {
                Separation = SeparationStyle.Strong,
                Columns = new List<Column>
                {
                    new Column
                    {
                        Size = "1",
                        Items = new List<CardElement>
                        {
                            new TextBlock
                            {
                                Text = title,
                                Size = TextSize.Large,
                                Weight = TextWeight.Bolder,
                                Wrap = true
                            },
                            new FactSet
                            {
                                Separation = SeparationStyle.None,
                                Facts = new List<AdaptiveCards.Fact>                                
                                {
                                    new AdaptiveCards.Fact("Search Score:", score),
                                    new AdaptiveCards.Fact("Category:", category),
                                }
                            }
                        }
                    },
                    new Column
                    {
                        Size = "auto",
                        Items = new List<CardElement>
                        {
                            new Image
                            {
                                Url = "https://bot-framework.azureedge.net/bot-icons-v1/bot-framework-default-7.png",
                                Size = ImageSize.Medium
                            }
                        }
                    }
                }
            };

            var textBlock = new TextBlock()
            {
                Text = text,
                Wrap = true,
                MaxLines = 2,
                Size = TextSize.Normal
            };

            var submitAction = new SubmitAction
            {
                Title = "More Details",
                Data = $"show me the article {title}"
            };
            
            // fill the card
            card.Body.Add(columnsBlock);
            card.Body.Add(textBlock);
            card.Actions.Add(submitAction);

            return card;
        }
    }
}