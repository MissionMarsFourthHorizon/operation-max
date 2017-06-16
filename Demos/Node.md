# Sample Demo Code (Node.js)

### Text Prompts

In the first step of the waterfall dialog we ask the user to describe his problem. In the second one, we receive what the user has entered and print it.

```javascript
var bot = new builder.UniversalBot(connector, [
    (session, args, next) => {
        builder.Prompts.text(session, 'First, please briefly describe your problem to me.');
    },
    (session, result, next) => {
        console.log(result.response);
    }
]);
```

### Choice, confirm, buttons in cards

In the first step of the waterfall dialog we ask the user to choose from a closed list the severity. In the second one, we receive what the user has selected and print it.

``` javascript
var bot = new builder.UniversalBot(connector, [
    (session, args, next) => {
        var choices = ['high', 'normal', 'low'];
        builder.Prompts.choice(session, 'Which is the severity of this problem?', choices, { listStyle: builder.ListStyle.button });
    },
    (session, result, next) => {
        console.log(result.response);
    }
]);
```

This code prompts the user for confirmation, expecting a yes/no answer.

``` javascript
var bot = new builder.UniversalBot(connector, [
    (session, args, next) => {
        builder.Prompts.confirm(session, 'Are you sure this is correct?', { listStyle: builder.ListStyle.button });
    },
    (session, result, next) => {
        console.log(result.response);
    }
]);
```

### TriggerAction

This dialog will response when the user send `Help` to the bot. We can put a RegEx in the `matches` property.

``` javascript
var bot = new builder.UniversalBot(connector, (session, args, next) => {
    session.endDialog(`I'm sorry, I did not understand '${session.message.text}'.\nType 'help' to know more about me :)`);
});

bot.dialog('Help',
    (session, args, next) => {
        session.endDialog(`I'm the help desk bot and I can help you create a ticket.\n` +
            `You can tell me things like _I need to reset my password_ or _I cannot print_.`);
    }
).triggerAction({
    matches: /(help|hi)/i
});
```

### Create an Adaptive Card

This dialog send an adaptive Card to the user. The card is arranged with a two rows: one for the title, one for the content. The content has two columns one for the ticket Category and Severity and one for the image.

``` javascript
var bot = new builder.UniversalBot(connector, [
    (session, args, next) => {
        session.send(new builder.Message(session).addAttachment({
            contentType: "application/vnd.microsoft.card.adaptive",
            content: {
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "type": "AdaptiveCard",
                "version": "1.0",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "Ticket #1",
                        "weight": "bolder",
                        "size": "large",
                    },
                    {
                        "type": "ColumnSet",
                        "columns": [
                            {
                                "type": "Column",
                                "size": "1",
                                "items": [
                                    {
                                        "type": "FactSet",
                                        "facts": [
                                            {
                                                "title": "Severity:",
                                                "value": "High"
                                            },
                                            {
                                                "title": "Category:",
                                                "value": "Software"
                                            }
                                        ]
                                    }
                                ]
                            },
                            {
                                "type": "Column",
                                "size": "auto",
                                "items": [
                                    {
                                        "type": "Image",
                                        "url": "https://raw.githubusercontent.com/GeekTrainer/help-desk-bot-lab/master/assets/botimages/head-smiling-medium.png",
                                        "horizontalAlignment": "right"
                                    }
                                ]
                            }
                        ],
                        "separation": "strong"
                    }
                ]
            }
        }));
    }
]);
```

## Exercise 7: Handoff to Human

### Middleware

The `botbuilder` method will log the messages the user sends to the bot and the `usersent` the messages the bot sends to the user. Here we plug the middleware after we initialize the `UniversalBot`.

``` javascript
const LoggingMiddleware = () => {
    return {
        botbuilder: (session, next) => {
            console.log(`Middleware logging: ${session.message.text}`);
            next();
        },
        usersent: function (event, next) {
            console.log(`Middleware logging: ${event.text}`);
            next();
        }
    };
};

bot.use(LoggingMiddleware());
```

### Send Typing Indicator

In this sample, we simulate a long running task with the `setTimeout` function and send to the user the `session.sendTyping()` to let she knows we are processing the response.

``` javascript
var bot = new builder.UniversalBot(connector, [
    (session, args, next) => {
        session.sendTyping();
        setTimeout(() => {
            session.send('You said: ' + session.message.text + ' which was ' + session.message.text.length + ' characters')
        },
        5000);
    }
]);
```

### Proactive Messages

In this sample, we accumulate the character length the user send in the message and when she is idle for 10 seconds the bot will inform the total character length she has entered. We must store the message address to send the message to the right place. More info [here](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-proactive-messages) about Proactive messages.

``` javascript
var charLen = 0;
var timeOutExecution;

function sendProactiveMessage(address) {
  var msg = new builder.Message().address(address);
  msg.text(`You have send ${charLen} characters so far.`);
  msg.textLocale('en-US');
  bot.send(msg);
}

var bot = new builder.UniversalBot(connector, [
    (session, args, next) => {
        var savedAddress = session.message.address;

        charLen += session.message.text.length;
        session.send('You said: ' + session.message.text + ' which was ' + session.message.text.length + ' characters');

        if (timeOutExecution) {
            clearTimeout(timeOutExecution);
        }

        timeOutExecution = setTimeout(() => {
            sendProactiveMessage(savedAddress);
            clearTimeout(timeOutExecution);
        }, 10000);
    }
]);
```

### Cortana Skill

You can interact with **Cortana**. You need to enable **Cortana Channel** in your bot in Bot Framework portal as described [here](https://docs.microsoft.com/en-us/bot-framework/debug-bots-cortana-skill-invoke#test-your-cortana-skill). In the following code, we use the `say` method instead `send`. This methods are similar but first one accepts a second parameter that is what the bot will speak. This parameter may be a SSML XML string to enrich the user experience.

``` javascript
var bot = new builder.UniversalBot(connector, [
    (session, args, next) => {
        session.say(`You said: _${session.message.text}_ which was _${session.message.text.length}_ characters`,
                    `You said: ${session.message.text} which was ${session.message.text.length} characters`);
    }
]);
```

### UserData

The following example prompts the user for their name and then saves it in [session.userData](https://docs.botframework.com/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session.html#userdata).
The message handler takes a `next()` function as an argument which we use to manually advance to the next step in the sequence of message handlers. This lets us check if we know the user's name and skips to the greeting if we do.

```javascript
var bot = new builder.UniversalBot(connector, [
    (session, args, next) => {
       if (!session.userData.name) {
            // Ask user for their name
            builder.Prompts.text(session, 'Please, tell me your name. I will remember.');
        } else {
            // Skip to next step
            next();
        }
    },
    (session, results) => {
        // Update name if answered
        if (results.response) {
            session.userData.name = results.response;
        }

        // Greet the user
        session.send(`Hi ${session.userData.name}!`);
    }
]);
```