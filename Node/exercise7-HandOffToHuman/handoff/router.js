/* jshint esversion: 6 */
const builder = require('botbuilder');
const { Provider, ConversationState } = require('./provider');

// dispatch messages between agent and user
function Router(bot, isAgent) {
    'use strict';

    const provider = new Provider();

    const middleware = () => {
        return {
            botbuilder: (session, next) => {
                if (session.message.type === 'message') {
                    if (isAgent(session)) {
                        routeAgentMessage(session);
                    } else {
                        routeUserMessage(session, next);
                    }
                } else {
                    next();
                }
            }
        };
    };

    const routeAgentMessage = (session) => {
        const message = session.message;
        const conversation = provider.findByAgentId(message.address.conversation.id);

        // if the agent is not in conversation, no further routing is necessary
        if (!conversation) {
            return;
        }

        // send text that agent typed to the user they are in conversation with
        bot.send(new builder.Message().address(conversation.user).text(message.text));
    };

    const routeUserMessage = (session, next) => {
        const message = session.message;

        const conversation = provider.findByConversationId(message.address.conversation.id) || provider.createConversation(message.address);

        // the next action depends on the conversation state
        switch (conversation.state) {
            case ConversationState.ConnectedToBot:
                // continue the normal bot flow
                return next();
            case ConversationState.WaitingForAgent:
                // send a notification to the customer
                session.send(`Connecting you to the next available human agent... please wait, there are ${pending()-1} users waiting.`);
                return;
            case ConversationState.ConnectedToAgent:
                // send text that customer typed to the agent they are in conversation with
                bot.send(new builder.Message().address(conversation.agent).text(message.text));
                return;
        }
    };

    const pending = () => {
        return provider.currentConversations().filter((conv) => conv.state === ConversationState.WaitingForAgent).length;
    };

    return {
        isAgent,
        middleware,
        pending,
        provider,
        bot
    };
}

module.exports = Router;