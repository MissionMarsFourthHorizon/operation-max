namespace HelpDeskBot.HandOff
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs.Internals;

    public static class AgentExtensions
    {
        private const string ISAGENT = "isAgent";

        public static bool IsAgent(this IBotData botData)
        {
            bool isAgent = false;
            botData.ConversationData.TryGetValue(ISAGENT, out isAgent);
            return isAgent;
        }

        public static Task SetAgent(this IBotData botData, bool value, CancellationToken token)
        {
            botData.ConversationData.SetValue(ISAGENT, value);
            return botData.FlushAsync(token);
        }
    }
}