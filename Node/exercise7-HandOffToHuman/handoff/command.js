/* jshint esversion: 6 */
const builder = require('botbuilder');
const { ConversationState } = require('./provider');

function Command(router) {
    'use strict';
    const provider = router.provider;

    const middleware = () => { return {
            botbuilder: (session, next) => {
                if (session.message.type === 'message' && router.isAgent(session)) {
                    agentCommand(session, next);
                } else {
                    next();
                }
            }
        };
    };

    const agentCommand = (session, next) => {
        const message = session.message;
        const conversation = provider.findByAgentId(message.address.conversation.id);

        if (/^agent help/i.test(message.text)) {
            return sendAgentHelp(session);
        }

        if (!conversation) {
            if (/^connect/i.test(message.text)) {
                // peek a conversation from the queue if any
                let targetConversation = provider.peekConversation(message.address);
                if (targetConversation) {
                    session.send('You are now connected to the next user that requested human help.\nType *resume* to connect the user back to the bot.');
                } else {
                    session.send('No users waiting in queue.');
                    return;
                }

                var hello = 'You are now talking to a human agent.';
                router.bot.send(new builder.Message().address(targetConversation.user).text(hello));
                return;
            } else {
                return sendAgentHelp(session);
            }
        } else {
            if (/^resume/i.test(message.text)) {
                // disconnect the user from the agent
                let targetConversation = provider.findByAgentId(message.address.conversation.id);
                targetConversation.state = ConversationState.ConnectedToBot;
                delete targetConversation.agent;
                session.send(`Disconnected. There are ${router.pending()} users waiting.`);

                var goodbye = 'You are now talking to the bot again.';
                router.bot.send(new builder.Message().address(targetConversation.user).text(goodbye));
                return;
            }
        }

        next();
    };

    const queueMe = (session) => {
        const message = session.message;
        // lookup the conversation (create it if one doesn't already exist)
        const conversation = provider.findByConversationId(message.address.conversation.id) || provider.createConversation(message.address);

        if (conversation.state == ConversationState.ConnectedToBot) {
            conversation.state = ConversationState.WaitingForAgent;
            return true;
        }

        return false;
    };

    return {
        middleware,
        queueMe
    };
}

const sendAgentHelp = (session) => {
    session.send('### Human Agent Help, please type:\n' +
                 ' - *connect* to connect to the user who has been waiting the longest.\n' +
                 ' - *agent help* at any time to see these options again.\n');
};

module.exports = Command;
