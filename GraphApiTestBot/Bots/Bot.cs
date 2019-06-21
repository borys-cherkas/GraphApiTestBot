// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphApiTestBot.Extensions;
using GraphApiTestBot.Middleware;
using GraphApiTestBot.Storage;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Graph;

namespace GraphApiTestBot.Bots
{
    public class Bot : ActivityHandler
    {
        private const string HelpCommand = "help";
        private const string ListFilesCommand = "list all files";
        
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //await turnContext.SendActivityAsync(MessageFactory.Text($"Borys CD Echo: {turnContext.Activity.Text}"), cancellationToken);
            var dialogInput = turnContext.Activity.Text?.Trim();
            if (string.IsNullOrEmpty(dialogInput))
            {
                await turnContext.SendActivityAsync($"Hello! Use one from commands listed below to start dialog.");
                await turnContext.SendActivityAsync($"['help', 'list all files']");
            }
            else if (dialogInput.Equals(HelpCommand, StringComparison.OrdinalIgnoreCase))
            {
                await ShowHelpDialogAsync(turnContext, cancellationToken);
            }
            else if (dialogInput.Equals(ListFilesCommand, StringComparison.OrdinalIgnoreCase))
            {
                await ShowListFilesDialogAsync(turnContext, cancellationToken);
            }
        }

        private async Task ShowHelpDialogAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync($"Available commands are only ones from the following set:");
            await turnContext.SendActivityAsync($"Help");
            await turnContext.SendActivityAsync($"List all files");
        }

        private async Task ShowListFilesDialogAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync($"Showing files here...");
            var tokenState = turnContext.TurnState.Get<ConversationAuthToken>(AzureAdAuthMiddleware.AuthTokenKey);
            try
            {
                var oneDriveItems = await MicrosoftGraphExtensions.GetMicrosoftGraphOneDriveFilesAsync(tokenState.AccessToken);
                StringBuilder sb = new StringBuilder("OneDrive stores such items: ");
                int counter = 1;
                foreach (var oneDriveItem in oneDriveItems)
                {
                    sb.AppendLine($"{counter++}: {oneDriveItem.Name} <{GetFileType(oneDriveItem)}>");
                }

                await turnContext.SendActivityAsync(sb.ToString());
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync($"Some error has been occured. {ex.Message}");
            }
        }

        private object GetFileType(DriveItem oneDriveItem)
        {
            if (oneDriveItem.File != null) return "file";
            if (oneDriveItem.Folder != null) return "file";
            return "unknown";
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and Welcome!"), cancellationToken);
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
        }
    }
}
