namespace HelpDeskBot
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Dialogs;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Services;

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly AzureSearchService searchService = new AzureSearchService();

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            else if (activity.Type == ActivityTypes.Event)
            {
                await this.HandleEventMessage(activity);
            }
            else
            {
                this.HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleEventMessage(Activity message)
        {
            if (string.Equals(message.Name, "showDetailsOf", StringComparison.InvariantCultureIgnoreCase))
            {
                var searchResult = await this.searchService.SearchByTitle(message.Value.ToString());
                string reply = "Sorry, I could not find that article.";

                if (searchResult != null && searchResult.Value.Length != 0)
                {
                    reply = "Maybe you can check this article first: \n\n" + searchResult.Value[0].Text;
                }

                // return our reply to the user
                Activity replyActivity = message.CreateReply(reply);

                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                await connector.Conversations.ReplyToActivityAsync(replyActivity);
            }
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}