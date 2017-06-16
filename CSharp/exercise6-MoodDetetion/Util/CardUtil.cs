namespace HelpDeskBot.Util
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Model;

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

                var cardImages = new CardImage[] { new CardImage("https://raw.githubusercontent.com/GeekTrainer/help-desk-bot-lab/master/assets/botimages/head-smiling-medium.png") };

                foreach (SearchResultHit item in searchResult.Value)
                {
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction button = new CardAction()
                    {
                        Value = $"show me the article {item.Title}",
                        Type = "postBack",
                        Title = "More details"
                    };
                    cardButtons.Add(button);

                    ThumbnailCard card = new ThumbnailCard()
                    {
                        Title = item.Title,
                        Subtitle = $"Category: {item.Category} | Search Score: {item.SearchScore}",
                        Text = item.Text.Substring(0, 50) + "...",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    reply.Attachments.Add(card.ToAttachment());
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
    }
}