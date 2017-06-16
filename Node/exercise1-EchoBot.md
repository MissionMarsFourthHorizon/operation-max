# Exercise 1: Creating Your First "Echo" Bot with the Bot Builder SDK for Node.js

## Introduction

This exercise will show you how to build a bot by using the Bot Builder SDK for Node.js and then test it with the Bot Framework Emulator.

Inside [this folder](./exercise1-EchoBot) you will find a solution with the code that results from completing the steps in this exercise. You can use this solutions as guidance if you need additional help as you work through this exercise. Remember that before using it, you first need to run `npm install`.

## Prerequisites

The following software is required for completing this exercise:

* [Latest Node.js with NPM](https://nodejs.org/en/download)
* A code editor like [Visual Studio Code](https://code.visualstudio.com/download) (preferred) or Visual Studio 2017 Community or higher
* [Bot Framework Emulator](https://emulator.botframework.com), which is the client you will use for testing your bot

## Task 1: Initialize the App and Install the Bot Builder SDK

The Bot Builder SDK for Node.js is a powerful, easy-to-use framework that provides a familiar way for Node.js developers to write bots. It leverages frameworks like Express & Restify to provide a familiar way for JavaScript developers to write bots.

1. To install the Bot Builder SDK and its dependencies, first create a folder for your bot. Open a console window, navigate to it, and run the following npm command. Use `app.js` as your entry point, and leave the rest of the default answers.

    ```
    npm init
    ```

1. Next, install the [Bot Builder SDK](https://dev.botframework.com), [Restify](http://restify.com/) and [Dotenv](https://github.com/motdotla/dotenv) modules by running the following npm commands:

    ```
    npm install --save botbuilder restify dotenv
    ```

    Bot Builder, part of Bot Framework, is used to create the bot, while Restify is used to serve the web application which will host your bot. Please notice that the Bot Builder SDK is independent of the Web framework you use. This hands-on lab uses Restify, but you can use others like Express or Koa. Dotenv is used to easily maintain all configuration settings in a separated file.

1. Install [Nodemon](https://nodemon.io/) as a global package. It will be used to run the bot and refresh whenever there are changes in the JavaScript files.

    ```
    npm install -g nodemon
    ```

## Task 2: Create the Bot

1. Create a file named `.env` in the root directory of the project with the following content. We will use this file to configure our bot.

    ```
    PORT=3978
    MICROSOFT_APP_ID=
    MICROSOFT_APP_PASSWORD=
    ```

1. Create a file named `app.js` in the root directory, which will become the root for your application, and your bot. Notice that the bot will be listening in port 3978 by default using the Restify framework, which is standard when developing bots.

    The code below has three main sections:
     * Creating the chat connector using the ChatConnector class
     * Using the connector in a Restify route to listen for messages
     * Adding the code using the UniversalBot class to reply to the user

    The Bot Builder SDK for Node.js provides the UniversalBot and ChatConnector classes for configuring the bot to send and receive messages through the Bot Framework Connector. The UniversalBot class forms the brains of your bot. It's responsible for managing all the conversations your bot has with a user. The ChatConnector connects your bot to the Bot Framework Connector Service. The Connector also normalizes the messages that the bot sends to channels so that you can develop your bot in a platform-agnostic way, allowing you to focus your attention on the business logic, rather than on the eventual channel a user might use.

    Add the following code to `app.js`:

    ``` javascript
    require('dotenv').config();
    const restify = require('restify');
    const builder = require('botbuilder');

    // Setup Restify Server
    var server = restify.createServer();
    server.listen(process.env.port || process.env.PORT || 3978, () => {
        console.log('%s listening to %s', server.name, server.url);
    });

    // Create chat connector for communicating with the Bot Framework Service
    var connector = new builder.ChatConnector({
        appId: process.env.MICROSOFT_APP_ID,
        appPassword: process.env.MICROSOFT_APP_PASSWORD
    });

    // Listen for messages from users
    server.post('/api/messages', connector.listen());

    // Receive messages from the user and respond by echoing each message back (prefixed with 'You said:')
    var bot = new builder.UniversalBot(connector, [
        (session, args, next) => {
            session.send('You said: ' + session.message.text + ' which was ' + session.message.text.length + ' characters');
        }
    ]);
    ```

## Task 3: Test the Bot

Next, test your bot by using the Bot Framework Emulator to see it in action. The emulator is a desktop application that lets you test and debug your bot on localhost or running remotely through a tunnel. The emulator displays messages as they would appear in a web chat UI and logs JSON requests and responses as you exchange messages with your bot.

1. Start your bot in a console window by using the following command. At this point, your bot is running locally.

    ```
    nodemon app.js
    ```

    > **NOTE:** If you get a Windows Firewall alert, click **Allow access**. Also, if you get an `EADDRINUSE` error, change the default port to 3979 or similar.

1. Next, start the Bot Framework Emulator, and then connect your bot. Type `http://localhost:3978/api/messages` into the address bar. This is the default endpoint that your bot listens to when hosted locally.

1. Set the **Locale** as `en-US` and click **Connect**. Because you are running your bot locally, you won't need to specify **Microsoft App ID** and **Microsoft App Password**. You can leave these fields blank for now. You'll get this information in Exercise 5 when you register your bot in the Bot Framework Portal.

1. You should see that the bot responds to each message you send by echoing back your message prefixed with the text "You said" and ending in the text "which was ## characters", where ## represents the number of characters in the user's message.

    ![exercise1-echo-bot](./images/exercise1-echo-bot.png)
