namespace HelpDeskBot.HandOff
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Builder.Scorables.Internals;
    using Microsoft.Bot.Connector;

    public class RouterScorable : ScorableBase<IActivity, ConversationReference, double>
    {
        private readonly ConversationReference conversationReference;
        private readonly Provider provider;
        private readonly IBotData botData;

        public RouterScorable(IBotData botData, ConversationReference conversationReference, Provider provider)
        {
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.conversationReference, nameof(conversationReference), conversationReference);
            SetField.NotNull(out this.provider, nameof(provider), provider);
        }

        protected override async Task<ConversationReference> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity.AsMessageActivity();

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                // determine if the message comes form an agent or user
                if (this.botData.IsAgent())
                {
                    return this.PrepareRouteableAgentActivity(message.Conversation.Id);
                }
                else
                {
                    return this.PrepareRouteableUserActivity(message.Conversation.Id);
                }
            }

            return null;
        }

        protected ConversationReference PrepareRouteableAgentActivity(string conversationId)
        {
            var conversation = this.provider.FindByAgentId(conversationId);
            return conversation?.User;
        }

        protected ConversationReference PrepareRouteableUserActivity(string conversationId)
        {
            var conversation = this.provider.FindByConversationId(conversationId);
            if (conversation == null)
            {
                conversation = this.provider.CreateConversation(this.conversationReference);
            }

            switch (conversation.State)
            {
                case ConversationState.ConnectedToBot:
                    return null; // continue normal flow
                case ConversationState.WaitingForAgent:
                    return conversation.User;
                case ConversationState.ConnectedToAgent:
                    return conversation.Agent;
            }

            return null;
        }

        protected override bool HasScore(IActivity item, ConversationReference destination)
        {
            return destination != null;
        }

        protected override double GetScore(IActivity item, ConversationReference destination)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, ConversationReference destination, CancellationToken token)
        {
            string textToReply;
            if (destination.Conversation.Id == this.conversationReference.Conversation.Id)
            {
                textToReply = "Connecting you to the next available human agent... please wait";
            }
            else
            {
                textToReply = item.AsMessageActivity().Text;
            }

            ConnectorClient connector = new ConnectorClient(new Uri(destination.ServiceUrl));
            var reply = destination.GetPostToUserMessage();
            reply.Text = textToReply;
            await connector.Conversations.SendToConversationAsync(reply);
        }

        protected override Task DoneAsync(IActivity item, ConversationReference state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
