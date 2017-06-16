/* jshint esversion: 6 */

// Conversation state enumeration
const ConversationState = {
    ConnectedToBot: 0,
    WaitingForAgent: 1,
    ConnectedToAgent: 2
};

function Provider () {
    'use strict';

    const data = [];

    // return all conversations
    const currentConversations = () => {
        return data;
    };

    // creates a new conversation
    const createConversation = (address) => {
        const conversation = {
            timestamp: new Date().getTime(),
            user: address,
            state: ConversationState.ConnectedToBot
        };

        data.push(conversation);

        return conversation;
    };

    // find a conversation by its user conversation id
    const findByConversationId = (id) => {
        return data.find((conversation) => conversation.user.conversation.id === id);
    };

    // find a conversation by its agent conversation id
    const findByAgentId = (id) => {
        return data.find((conversation) => conversation.agent && conversation.agent.conversation.id === id);
    };

    // find a conversation by its agent conversation id
    const peekConversation = (agent) => {
        var conversation = data.sort((a,b) => a.timestamp < b.timestamp).find((conversation) => conversation.state === ConversationState.WaitingForAgent);
        if (conversation) {
            conversation.state = ConversationState.ConnectedToAgent;
            conversation.agent = agent;
        }
        return conversation;
    };

    return {
        createConversation,
        currentConversations,
        findByAgentId,
        findByConversationId,
        peekConversation
    };
}

module.exports = { Provider, ConversationState };