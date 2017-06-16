namespace HelpDeskBot.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HandOff;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Builder.Scorables.Internals;
    using Microsoft.Bot.Connector;

    public class AgentLoginScorable : ScorableBase<IActivity, string, double>
    {
        private const string TRIGGER = "/agent login";
        private readonly Provider provider;
        private readonly IBotData botData;

        public AgentLoginScorable(IBotData botData, Provider provider)
        {
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.provider, nameof(provider), provider);
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }

        protected async override Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            await this.botData.SetAgent(true, token);

            ConnectorClient connector = new ConnectorClient(new Uri(item.ServiceUrl));
            var welcome = $"Welcome back human agent, there are {this.provider.Pending()} users waiting in the queue.\n\nType _agent help_ for more details.";
            Activity reply = ((Activity)item).CreateReply(welcome);

            await connector.Conversations.ReplyToActivityAsync(reply, token);
        }

        protected async override Task<string> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item.AsMessageActivity();
            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                if (message.Text.Equals(TRIGGER, StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text;
                }
            }

            return null;
        }
    }
}