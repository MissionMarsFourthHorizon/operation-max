# Exercise 2: Submitting Help Desk Tickets with the Bot (C#)

## Introduction

In this exercise you will learn how to add conversation abilities to the bot to guide the user to create a help desk ticket.

Inside [this folder](./exercise2-TicketSubmissionDialog) you will find a solution with the code that results from completing the steps in this exercise. You can use this solution as guidance if you need additional help as you work through this exercise.

## Prerequisites

The following software is required for completing this exercise:

* [Visual Studio 2017 Community](https://www.visualstudio.com/downloads) or higher
* The [Bot Framework Emulator](https://emulator.botframework.com) (make sure it's configured with the `en-US` Locale)

## Task 1: Adding Conversation to the Bot

In this task you will modify the bot code to ask the user a sequence of questions before performing some action.

1. Open the solution you've obtained from the previous exercise. Alternatively, you can open the solution from the [exercise1-EchoBot](./exercise1-EchoBot) folder.

1. Open the **Dialogs\RootDialog.cs** file.

1. Add the following variables at the beginning of the `RootDialog` class. We will use them later to store the user answers.

    ```csharp
    private string category;
    private string severity;
    private string description;
    ```

1. Replace the method `MessageReceivedAsync` with the following code.

    ```csharp
    public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        await context.PostAsync("Hi! I’m the help desk bot and I can help you create a ticket.");
        PromptDialog.Text(context, this.DescriptionMessageReceivedAsync, "First, please briefly describe your problem to me.");
    }

    public async Task DescriptionMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
    {
        this.description = await argument;
        await context.PostAsync($"Got it. Your problem is \"{this.description}\"");
        context.Done<object>(null);
    }
    ```

    You will notice the Dialog implementation consist of a set of methods that are connected together using either the [conversation flow control](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-manage-conversation-flow#a-iddialog-lifecyclea-dialog-lifecycle) methods (provided by the `IDialogContext` interface) or some of the `PromptDialog` helper methods which also use the `IDialogContext` methods behind the scene to manage the conversation flow.

    When the conversation first starts, the dialog does not contain state, so the `Conversation.SendAsync` constructs `RootDialog` and calls its `StartAsync` method. The `StartAsync` method calls `IDialogContext.Wait` with the continuation delegate to specify the method that should be called when a new message is received (in this case is the `MessageReceivedAsync` method).

    The Bot Framework SDK provides a set of built-in prompts to simplify collecting input from a user. The `MessageReceivedAsync` method waits for a message, which once received, posts a response greeting the user and calls `PromptDialog.Text()` to prompt him to describe the problem.

    Also, the response is persisted in the dialog instance by the framework. Notice it was marked as `[Serializable]`. This is essential for storing temporary information in between the steps of the dialog.

1. Run the solution in Visual Studio (click the **Run** button) and open the emulator. Type the bot URL as usual (`http://localhost:3979/api/messages`) and test the bot as show below.

    ![exercise2-dialog](./images/exercise2-dialog.png)

## Task 2: Prompting for the Tickets Details

In this task you are going to add more message handlers to the bot code to prompt for all the ticket details.

1. Stop the app and open the **Dialogs\RootDialog.cs** file.

1. Update the `DescriptionMessageReceivedAsync` to store the description the user entered and prompt the ticket's severity. The following code uses the `PromptDialog.Choice` method which will give the user a set of choices to pick.

    ``` csharp
    public async Task DescriptionMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
    {
        this.description = await argument;
        var severities = new string[] { "high", "normal", "low" };
        PromptDialog.Choice(context, this.SeverityMessageReceivedAsync, severities, "Which is the severity of this problem?");
    }
    ```

1. Next, add the `SeverityMessageReceivedAsync` method that receives the severity and prompts the user to enter the category using the `PromptDialog.Text` method.

    ``` csharp
    public async Task SeverityMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
    {
        this.severity = await argument;
        PromptDialog.Text(context, this.CategoryMessageReceivedAsync, "Which would be the category for this ticket (software, hardware, networking, security or other)?");
    }
    ```

1. Now add the `CategoryMessageReceivedAsync` method which stores the category and prompt the user to confirm the ticket creation using the `PromptDialog.Confirm` method.

    ``` csharp
    public async Task CategoryMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
    {
        this.category = await argument;
        var text = $"Great! I'm going to create a \"{this.severity}\" severity ticket in the \"{this.category}\" category. " +
                    $"The description I will use is \"{this.description}\". Can you please confirm that this information is correct?";

        PromptDialog.Confirm(context, this.IssueConfirmedMessageReceivedAsync, text);
    }
    ```

    > **NOTE:** Notice that you can use Markdown syntax to create richer text messages. However it's important to note that not all channels support Markdown.

1. Add a method to handle the response from the confirmation message.

    ``` csharp
    public async Task IssueConfirmedMessageReceivedAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirmed = await argument;

        if (confirmed)
        {
            await context.PostAsync("Awesome! Your ticked has been created.");
        }
        else
        {
            await context.PostAsync("Ok. The ticket was not created. You can start again if you want.");
        }

        context.Done<object>(null);
    }
    ```

1. Re-run the app and use the 'Start new conversation' button of the emulator ![exercise2-start-new](./images/exercise2-start-new.png). Test the new conversation.

    ![exercise2-full-conversation-1](./images/exercise2-full-conversation-1.png)

    > **NOTE:** At this point if you talk to the bot again, the dialog will start over.

## Task 3: Calling an External API to Save the Ticket

Now you have all the information for the ticket, however that information is discarded when the dialog ends. You will now add the code to create the ticket using an external API. For simplicity purposes, you will use a simple endpoint that saves the ticket into an in-memory array. In the real world, you would use any external API that is accessible from your bot's code.

> **NOTE:** One important fact about bots to keep in mind is most bots you will build will be a front end to an existing API. Bots are simply apps, and they do not require artificial intelligence (AI), machine learning (ML), or natural language processing (NLP), to be considered a bot.

1. Stop the app. In the **Controllers** folder copy the [TicketsController.cs](../assets/exercise2-TicketSubmissionDialog/TicketsController.cs) from the [assets](../assets) folder of this hands-on lab. This will handle the **POST** request to the `/api/tickets` endpoint, add the ticket to an array and respond with the _ticket id_ created.

1. Add a new `Util` folder to your project. In the new folder,
copy the [TicketAPIClient.cs](../assets/exercise2-TicketSubmissionDialog/TicketAPIClient.cs) file which will call the ticket API from the bot.

1. Update your `Web.Config` file by adding the key **TicketsAPIBaseUrl** under the **appSettings** section. This key will contain the Base URL where the Ticket API will run. In this exercise, it will be the same URL where the bot is running, but in a real world scenario it may be different URL.

    ``` xml
    <add key="TicketsAPIBaseUrl" value="http://localhost:3979/" />
    ```

1. Open the **Dialogs\RootDialog.cs** file.

1. Add the `HelpDeskBot.Util` using statements.

    ```csharp
    using HelpDeskBot.Util;
    ```

1. Replace the content of the `IssueConfirmedMessageReceivedAsync` method to make the call using the **TicketAPIClient**.

    ``` csharp
    public async Task IssueConfirmedMessageReceivedAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirmed = await argument;

        if (confirmed)
        {
            var api = new TicketAPIClient();
            var ticketId = await api.PostTicketAsync(this.category, this.severity, this.description);

            if (ticketId != -1)
            {
                await context.PostAsync($"Awesome! Your ticked has been created with the number {ticketId}.");
            }
            else
            {
                await context.PostAsync("Ooops! Something went wrong while I was saving your ticket. Please try again later.");
            }
        }
        else
        {
            await context.PostAsync("Ok. The ticket was not created. You can start again if you want.");
        }

        context.Done<object>(null);
    }
    ```

1. Re-run the app and use the _Start new conversation_ button of the emulator ![](./images/exercise2-start-new.png). Test the full conversation again to check that the ticket id is returned from the API.

    ![exercise2-full-conversation-2](./images/exercise2-full-conversation-2.png)

## Task 4: Change notification message to show an Adaptive Card

In this task you will enhance the confirmation message that is shown to the user after the ticket using [Adaptive Cards](http://adaptivecards.io/). Adaptive Cards are an open card exchange format enabling developers to exchange UI content in a common and consistent way. Their content can be specified as a JSON object. Content can then be rendered natively inside a host application (Bot Framework channels), automatically adapting to the look and feel of the host.

1. You will need to add the `Microsoft.AdaptiveCards` NuGet package. Right click on your project's **References** folder in the **Solution Explorer** and click **Manage NuGet packages**. Search for the `Microsoft.AdaptiveCards` and then click on the **Install** button. Or you can type in the **Packager Manager Console** `Install-Package Microsoft.AdaptiveCards`.

1. Open the **Dialogs\RootDialog.cs** file.

1. Add the `System.Collections.Generic` and `AdaptiveCards` using statements.

    ```csharp
    using System.Collections.Generic;
    using AdaptiveCards;
    ```

1. At the end of the file (inside the `RootDialog` class) add the following code that creates the Adaptive card:

    > Anatomy of this example card,
    > 1. A header section containing the title with the _ticketID_
    > 1. A middle section containing a `ColumnSet` with two columns: one for a `FactSet` with the _Severity_ and _Category_ and another with an icon
    > 1. A last section that includes a description block with the ticket description

    ``` csharp
    private AdaptiveCard CreateCard(int ticketId, string category, string severity, string description)
    {
        AdaptiveCard card = new AdaptiveCard();

        var headerBlock = new TextBlock()
        {
            Text = $"Ticket #{ticketId}",
            Weight = TextWeight.Bolder,
            Size = TextSize.Large,
            Speak = $"<s>You've created a new Ticket #{ticketId}</s><s>We will contact you soon.</s>"
        };

        var columnsBlock = new ColumnSet()
        {
            Separation = SeparationStyle.Strong,
            Columns = new List<Column>
            {
                new Column
                {
                    Size = "1",
                    Items = new List<CardElement>
                    {
                        new FactSet
                        {
                            Facts = new List<AdaptiveCards.Fact>
                            {
                                new AdaptiveCards.Fact("Severity:", severity),
                                new AdaptiveCards.Fact("Category:", category),
                            }
                        }
                    }
                },
                new Column
                {
                    Size = "auto",
                    Items = new List<CardElement>
                    {
                        new Image
                        {
                            Url = "https://raw.githubusercontent.com/GeekTrainer/help-desk-bot-lab/master/assets/botimages/head-smiling-medium.png",
                            Size = ImageSize.Small,
                            HorizontalAlignment = HorizontalAlignment.Right
                        }
                    }
                }
            }
        };

        var descriptionBlock = new TextBlock
        {
            Text = description,
            Wrap = true
        };

        card.Body.Add(headerBlock);
        card.Body.Add(columnsBlock);
        card.Body.Add(descriptionBlock);

        return card;
    }
    ```

1. Update the `IssueConfirmedMessageReceivedAsync` method to call this method when the ticket was successfully created.

    ``` csharp
    public async Task IssueConfirmedMessageReceivedAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirmed = await argument;

        if (confirmed)
        {
            var api = new TicketAPIClient();
            var ticketId = await api.PostTicketAsync(this.category, this.severity, this.description);

            if (ticketId != -1)
            {
                var message = context.MakeMessage();
                message.Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = CreateCard(ticketId, this.category, this.severity, this.description)
                    }
                };
                await context.PostAsync(message);
            }
            else
            {
                await context.PostAsync("Ooops! Something went wrong while I was saving your ticket. Please try again later.");
            }
        }
        else
        {
            await context.PostAsync("Ok. The ticket was not created. You can start again if you want.");
        }
        context.Done<object>(null);
    }
    ```

1. Re-run the app and use the _Start new conversation_ button of the emulator ![](./images/exercise2-start-new.png). Test the new conversation. You should see the Adaptive Card as follows.

    ![exercise2-emulator-adaptivecards](./images/exercise2-emulator-adaptivecards.png)

## Further Challenges

If you want to continue working on your own you can try with these tasks:

* Send a welcome message to the bot relying on the `conversationUpdate` event, as explained [here](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-activities#conversationupdate).
* Send a typing indicator to the bot while it calls the Tickets API, as explained [here](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-activities#typing).
* Update the data store for the trouble tickets to use a database, such as SQL Server, MongoDB, or Cosmos DB.