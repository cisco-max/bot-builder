// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const dotenv = require('dotenv');
const path = require('path');
const restify = require('restify');
const { ResourceExplorer } = require('botbuilder-dialogs-declarative');
const { AdaptiveDialogComponentRegistration, LanguageGeneratorExtensions, ResourceExtensions } = require('botbuilder-dialogs-adaptive');
const { OrchestratorComponentRegistration } = require('botbuilder-ai-orchestrator');
const { DialogManager } = require('botbuilder-dialogs');
const { MemoryStorage, UserState, ConversationState } = require('botbuilder');
const settings = require('./generated/orchestrator.settings.json');

console.log(`settings: ${settings.orchestrator.modelPath}`);
console.log(`process: ${process.env.modelPath}`);

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { BotFrameworkAdapter } = require('botbuilder');

// Import required bot configuration.
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

// Set up resource explorer
const resourceExplorer = new ResourceExplorer().addFolder(__dirname, true, true);
resourceExplorer.addComponent(new AdaptiveDialogComponentRegistration(resourceExplorer));
resourceExplorer.addComponent(new OrchestratorComponentRegistration(resourceExplorer));

// Create HTTP server
const server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, () => {
    console.log(`\n${ server.name } listening to ${ server.url }`);
    console.log('\nGet Bot Framework Emulator: https://aka.ms/botframework-emulator');
    console.log('\nTo talk to your bot, open the emulator select "Open Bot"');
});

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about how bots work.
const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword
});

// Catch-all for errors.
const onTurnErrorHandler = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights.
    console.error(`\n [onTurnError] unhandled error: ${ error }`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${ error }`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Send a message to the user
    await context.sendActivity('The bot encountered an error or bug.');
    await context.sendActivity('To continue to run this bot, please fix the bot source code.');
};

// Set the onTurnError for the singleton BotFrameworkAdapter.
adapter.onTurnError = onTurnErrorHandler;

// Define the state store for your bot.
// See https://aka.ms/about-bot-state to learn more about using MemoryStorage.
// A bot requires a state storage system to persist the dialog and user state between messages.
const memoryStorage = new MemoryStorage();

// Create conversation state with in-memory storage provider.
const conversationState = new ConversationState(memoryStorage);
const userState = new UserState(memoryStorage);

let myBot;

const loadRootDialog = () => {
    console.log('(Re)Loading dialogs...');
    // Load root dialog
    myBot = new DialogManager(resourceExplorer.loadType('RootDialog.dialog'));
    myBot.userState = userState;
    myBot.conversationState = conversationState;
    myBot.initialTurnState.set("settings", settings);
    ResourceExtensions.useResourceExplorer(myBot);
    LanguageGeneratorExtensions.useLanguageGeneration(myBot);
    ResourceExtensions.useResourceExplorer(myBot, resourceExplorer);
};

loadRootDialog();

// Listen for incoming requests.
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        // Route to main dialog.
        await myBot.onTurn(context);
    });
});

// Listen for Upgrade requests for Streaming.
server.on('upgrade', (req, socket, head) => {
    // Create an adapter scoped to this WebSocket connection to allow storing session data.
    const streamingAdapter = new BotFrameworkAdapter({
        appId: process.env.MicrosoftAppId,
        appPassword: process.env.MicrosoftAppPassword
    });
    // Set onTurnError for the BotFrameworkAdapter created for each connection.
    streamingAdapter.onTurnError = onTurnErrorHandler;

    streamingAdapter.useWebSocket(req, socket, head, async (context) => {
        // After connecting via WebSocket, run this logic for every request sent over
        // the WebSocket connection.
        await myBot.onTurn(context);
    });
});

// Add a resource change handler to resource explorer.
resourceExplorer.changed = async (_event, resources) => {
    if ((resources || []).find(r => r.id.endsWith('.dialog')) !== undefined) {
        loadRootDialog();
    }
};
