﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

<<<<<<< HEAD
=======
using System;
>>>>>>> parent of 9ef6b974 (ME Search Issue fixes)
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsMessagingExtensionsSearchBot : TeamsActivityHandler
    {
<<<<<<< HEAD
        public readonly string _baseURL;
        public TeamsMessagingExtensionsSearchBot(IConfiguration configuration):base()
        {
            this._baseURL = configuration["BaseUrl"];
=======
        private readonly IConfiguration _configuration;
        public TeamsMessagingExtensionsSearchBot(IConfiguration configuration)
        {
            _configuration = configuration;
>>>>>>> parent of 9ef6b974 (ME Search Issue fixes)
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var text = query?.Parameters?[0]?.Value as string ?? string.Empty;

            switch (text)
            {
                case "adaptive card":
                    MessagingExtensionResponse response = GetAdaptiveCard();
                    return response;

                case "connector card":
                    MessagingExtensionResponse connectorCard = GetConnectorCard();
                    return connectorCard;

                case "result grid":
                    MessagingExtensionResponse resultGrid = GetResultGrid();
                    return resultGrid;
            }

            var packages = await FindPackages(text);

            // We take every row of the results and wrap them in cards wrapped in MessagingExtensionAttachment objects.
            // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.
            var attachments = packages.Select(package =>
            {
                var previewCard = new ThumbnailCard { Title = package.Item1, Tap = new CardAction { Type = "invoke", Value = package } };
                if (!string.IsNullOrEmpty(package.Item5))
                {
                    previewCard.Images = new List<CardImage>() { new CardImage(package.Item5, "Icon") };
                }

                var attachment = new MessagingExtensionAttachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard { Title = package.Item1 },
                    Preview = previewCard.ToAttachment()
                };

                return attachment;
            }).ToList();

            // The list of MessagingExtensionAttachments must we wrapped in a MessagingExtensionResult wrapped in a MessagingExtensionResponse.
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments
                }
            };
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            // The Preview card's Tap should have a Value property assigned, this will be returned to the bot in this event. 
            var (packageId, version, description, projectUrl, iconUrl) = query.ToObject<(string, string, string, string, string)>();

            // We take every row of the results and wrap them in cards wrapped in in MessagingExtensionAttachment objects.
            // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.

            var card = new ThumbnailCard
            {
                Title = $"{packageId}, {version}",
                Subtitle = description,
                Buttons = new List<CardAction>
                    {
                        new CardAction { Type = ActionTypes.OpenUrl, Title = "Nuget Package", Value = $"https://www.nuget.org/packages/{packageId}" },
                        new CardAction { Type = ActionTypes.OpenUrl, Title = "Project", Value = projectUrl },
                    },
            };

            if (!string.IsNullOrEmpty(iconUrl))
            {
                card.Images = new List<CardImage>() { new CardImage(iconUrl, "Icon") };
            }

            var attachment = new MessagingExtensionAttachment
            {
                ContentType = ThumbnailCard.ContentType,
                Content = card,
            };

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            });
        }

        // Generate a set of substrings to illustrate the idea of a set of results coming back from a query. 
        private async Task<IEnumerable<(string, string, string, string, string)>> FindPackages(string text)
        {
            var obj = JObject.Parse(await (new HttpClient()).GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{text}&prerelease=true"));
            return obj["data"].Select(item => (item["id"].ToString(), item["version"].ToString(), item["description"].ToString(), item["projectUrl"]?.ToString(), item["iconUrl"]?.ToString()));
        }

        public MessagingExtensionResponse GetAdaptiveCard()
        {

<<<<<<< HEAD
            string filepath = "./RestaurantCard.json";
=======
            string filepath = "./Resources/RestaurantCard.json";
>>>>>>> parent of 9ef6b974 (ME Search Issue fixes)
            var previewcard = new ThumbnailCard
            {
                Title = "Adaptive Card",
                Text = "Please select to get Adaptive card"
            };
<<<<<<< HEAD
            var adaptiveList = FetchAdaptive(filepath);

=======

            var adaptiveList = FetchAdaptiveCard(filepath);
>>>>>>> parent of 9ef6b974 (ME Search Issue fixes)
            var attachment = new MessagingExtensionAttachment
            {
                ContentType = AdaptiveCards.AdaptiveCard.ContentType,
                Content = adaptiveList.Content,
                Preview = previewcard.ToAttachment()
            };

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            };
        }

        public MessagingExtensionResponse GetConnectorCard()
        {
<<<<<<< HEAD
          
            string filepath = "./connectorCard.json";
=======
            string filepath = "./Resources/connectorCard.json";
>>>>>>> parent of 9ef6b974 (ME Search Issue fixes)
            var json = File.ReadAllText(filepath);
            var cardJson = JsonConvert.DeserializeObject<ConnectorJsonSerializer>(json);
<<<<<<< HEAD
            cardJson.sections[0].activityImage = _baseURL+"/imgConnector.jpg";
=======

            cardJson.sections[0].activityImage = _configuration + "/imgConnector.jpg";

>>>>>>> parent of 9ef6b974 (ME Search Issue fixes)
            var ConnectorCardJson = JsonConvert.SerializeObject(cardJson);
            var previewcard = new ThumbnailCard
            {
                Title = "O365 Connector Card",
                Text = "Please select to get Connector card"
            };

            var connector = FetchConnectorCard(filepath);
            var attachment = new MessagingExtensionAttachment
            {
                ContentType = O365ConnectorCard.ContentType,
                Content = connector.Content,
                Preview = previewcard.ToAttachment()
            };

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            };
        }

<<<<<<< HEAD
        public static Attachment FetchAdaptive(string filepath)
=======
        public static Attachment FetchAdaptiveCard(string filepath)
>>>>>>> parent of 9ef6b974 (ME Search Issue fixes)
        {
            var adaptiveCardJson = File.ReadAllText(filepath);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return adaptiveCardAttachment;
        }

<<<<<<< HEAD
        public Attachment FetchConnector(string cardJson)
        {
=======
        public Attachment FetchConnectorCard(string filepath)
        {
            var connectorCardJson = File.ReadAllText(filepath);
>>>>>>> parent of 9ef6b974 (ME Search Issue fixes)
            var connectorCardAttachment = new MessagingExtensionAttachment
            {
                ContentType = O365ConnectorCard.ContentType,
                Content = JsonConvert.DeserializeObject(connectorCardJson),

            };
            
            return connectorCardAttachment;
        }

        public MessagingExtensionResponse GetResultGrid()
        {
            var files = Directory.GetFiles("wwwroot");
            List<string> imageFiles = new List<string>();

            foreach (string filename in files)
            {
                if (Regex.IsMatch(filename, @".jpg"))
                    imageFiles.Add(filename);

            }

            List<MessagingExtensionAttachment> attachments = new List<MessagingExtensionAttachment>();

            foreach (string img in imageFiles)
            {
                var image = img.Split("\\");           
                var thumbnailCard = new ThumbnailCard();
<<<<<<< HEAD
                thumbnailCard.Images = new List<CardImage>() { new CardImage(_baseURL +"/" + image[1]) };
=======
                thumbnailCard.Images = new List<CardImage>() { new CardImage(_configuration["BaseUrl"]+ image[1]) };
>>>>>>> parent of 9ef6b974 (ME Search Issue fixes)
                var attachment = new MessagingExtensionAttachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = thumbnailCard,
                };
                attachments.Add(attachment);

            }

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "grid",
                    Attachments = attachments
                }
            };

        }
    }
}
