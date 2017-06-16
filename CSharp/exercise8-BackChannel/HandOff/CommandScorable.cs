namespace HelpDeskBot.HandOff
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Builder.Scorables.Internals;
    using Microsoft.Bot.Connector;

    public enum AgentCommand
    {
        Help,
        Connect,
        Resume,
        None
    }

    public class CommandScorable : ScorableBase<IActivity, AgentCommand, double>
    {
        private const string AgentCommandOptions =
            "### Human Agent Help, please type:\n" +
            " - *connect* to connect to the user who has been waiting the longest.\n" +
            " - *agent help* at any time to see these options again.\n";

        private readonly ConversationReference conversationReference;
        private readonly Provider provider;
        private readonly IBotData botData;

        public CommandScorable(IBotData botData, ConversationReference conversationReference, Provider provider)
        {
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.conversationReference, nameof(conversationReference), conversationReference);
            SetField.NotNull(out this.provider, nameof(provider), provider);
        }

        protected override async Task<AgentCommand> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity.AsMessageActivity();

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                // determine if the message comes form an agent or user
                if (this.botData.IsAgent())
                {
                    if (message.Text.Equals("agent help", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return AgentCommand.Help;
                    }
                    else
                    {
                        var conversation = this.provider.FindByAgentId(message.Conversation.Id);
                        if (conversation == null)
                        {
                            if (message.Text.Equals("connect", StringComparison.InvariantCultureIgnoreCase))
                            {
                                return AgentCommand.Connect;
                            }
                        }
                        else
                        {
                            if (message.Text.Equals("resume", StringComparison.InvariantCultureIgnoreCase))
                            {
                                return AgentCommand.Resume;
                            }
                        }
                    }
                }
            }

            return AgentCommand.None;
        }

        protected override bool HasScore(IActivity item, AgentCommand state)
        {
            return state != AgentCommand.None;
        }

        protected override double GetScore(IActivity item, AgentCommand state)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, AgentCommand state, CancellationToken token)
        {
            var message = item as IMessageActivity;
            var connectorAgent = new ConnectorClient(new Uri(message.ServiceUrl));
            ConnectorClient connectorUser = null;
            Conversation targetConversation = null;

            var messageToAgent = string.Empty;
            var messageToUser = string.Empty;

            switch (state)
            {
                case AgentCommand.Help:
                    messageToAgent = AgentCommandOptions;
                    break;
                case AgentCommand.Connect:
                    targetConversation = this.provider.PeekConversation(this.conversationReference);
                    if (targetConversation != null)
                    {
                        messageToUser = "You are now talking to a human agent.";
                        connectorUser = new ConnectorClient(new Uri(targetConversation.User.ServiceUrl));

                        messageToAgent = "You are now connected to the next user that requested human help.\nType *resume* to connect the user back to the bot.";
                    }
                    else
                    {
                        messageToAgent = "No users waiting in queue.";
                    }

                    break;
                case AgentCommand.Resume:
                    targetConversation = this.provider.FindByAgentId(message.Conversation.Id);
                    targetConversation.State = ConversationState.ConnectedToBot;
                    targetConversation.Agent = null;

                    messageToUser = "You are now talking to the bot again.";
                    connectorUser = new ConnectorClient(new Uri(targetConversation.User.ServiceUrl));

                    messageToAgent = $"Disconnected. There are {this.provider.Pending()} users waiting.";
                    break;
            }

            if (connectorUser != null && targetConversation != null && !string.IsNullOrEmpty(messageToUser))
            {
                var replyToUser = targetConversation.User.GetPostToUserMessage();
                replyToUser.Text = messageToUser;
                await connectorUser.Conversations.SendToConversationAsync(replyToUser);
            }

            var replyToAgent = ((Activity)item).CreateReply(messageToAgent);
            await connectorAgent.Conversations.ReplyToActivityAsync(replyToAgent);
        }

        protected override Task DoneAsync(IActivity item, AgentCommand state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
