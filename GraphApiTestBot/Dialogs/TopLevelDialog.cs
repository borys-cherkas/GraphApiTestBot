using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphApiTestBot.CardsTemplates;
using GraphApiTestBot.Extensions;
using GraphApiTestBot.Graph;
using GraphApiTestBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace GraphApiTestBot.Dialogs
{
    public class TopLevelDialog : ComponentDialog
    {
        private const string HelpCommand = "help";
        private const string ProfileInfoCommand = "profile info";
        private const string ListOneDriveItemsCommand = "list my items";

        private readonly List<string> _signInRequiredCommands = new List<string> { ProfileInfoCommand, ListOneDriveItemsCommand };

        public TopLevelDialog(IConfiguration configuration, UserState userState, ConversationState conversationState) 
            : base(nameof(TopLevelDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new SignInDialog(configuration));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                OpenActionChooserAsync,
                OpenSignInDialogAsync,
                HandleSignInResultAsync,
                ExecuteChosenActionAsync,
                AskSomethingMoreAsync,
                HandleSomethingMoreQuestingAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        // Waterfall Step 1
        private async Task<DialogTurnResult> OpenActionChooserAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Would you like to do?"),
                    RetryPrompt = MessageFactory.Text("There's no such option. Please, choose one from suggested list."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { HelpCommand, ProfileInfoCommand, ListOneDriveItemsCommand }),
                }, cancellationToken);
        }

        // Waterfall Step 2
        private async Task<DialogTurnResult> OpenSignInDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var command = ((FoundChoice)stepContext.Result).Value;
            stepContext.Values["command"] = command;

            if (!_signInRequiredCommands.Contains(command))
            {
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            return await stepContext.BeginDialogAsync(nameof(SignInDialog), null, cancellationToken);
        }

        // Waterfall Step 3
        private async Task<DialogTurnResult> HandleSignInResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var command = (string) stepContext.Values["command"];
            if (_signInRequiredCommands.Contains(command))
            {
                var accessTokenState = (TokenState)stepContext.Result;
                stepContext.Values["accessToken"] = accessTokenState;
            }

            return await stepContext.NextAsync(command, cancellationToken: cancellationToken);
        }

        // Waterfall Step 4
        private async Task<DialogTurnResult> ExecuteChosenActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (string)stepContext.Result;

            switch (message.ToLower().Trim())
            {
                case HelpCommand:
                {
                    await ShowHelpCardAsync(stepContext, cancellationToken);
                    break;
                }
                case ProfileInfoCommand:
                {
                    await LoadAndShowProfileInfoAsync(stepContext, cancellationToken);
                    break;
                }
                case ListOneDriveItemsCommand:
                {
                    await LoadAndShowOneDriveItemsAsync(stepContext, cancellationToken);
                    break;
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task ShowHelpCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await Cards.ShowActivityWithAttachmentsAsync(
                stepContext.Context, 
                Cards.BuildHelpAttachmentList(),
                cancellationToken: cancellationToken);
        }

        private async Task LoadAndShowProfileInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenState = (TokenState)stepContext.Values["accessToken"];
            var currentUser = await GraphApiHelper.GetCurrentUserAsync(tokenState, cancellationToken);

            await stepContext.Context.SendActivityAsync($"You are {currentUser.DisplayName}.", cancellationToken: cancellationToken);
        }

        private async Task LoadAndShowOneDriveItemsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenState = (TokenState)stepContext.Values["accessToken"];
            var oneDriveItems = await GraphApiHelper.GetOneDriveFilesListAsync(tokenState, cancellationToken);

            const string downloadUrlKey = "@microsoft.graph.downloadUrl";
            for (int i = 0; i < 3; i++)
            {
                var driveItem = oneDriveItems.ToList()[i];
                await stepContext.Context.SendActivityAsync($"{i}: driveItem.AdditionalData != null = {driveItem.AdditionalData != null}.", cancellationToken: cancellationToken);
                
                if (driveItem.AdditionalData != null)
                {
                    await stepContext.Context.SendActivityAsync($"{i}: driveItem.AdditionalData.ContainsKey(downloadUrlKey) = {driveItem.AdditionalData.ContainsKey(downloadUrlKey)}.", cancellationToken: cancellationToken);

                    foreach (var kv in driveItem.AdditionalData)
                    {
                        await stepContext.Context.SendActivityAsync($"{i}: key = {kv.Key}, value = {kv.Value}.", cancellationToken: cancellationToken);
                    }
                }

            }

            await Cards.ShowActivityWithAttachmentsAsync(
                    stepContext.Context,
                    Cards.BuildOneDriveAttachmentList(oneDriveItems),
                    cancellationToken: cancellationToken);
        }

        // Waterfall Step 5
        private async Task<DialogTurnResult> AskSomethingMoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Would you like to perform another operation?")
                }, cancellationToken);
        }

        // Waterfall Step 6
        private async Task<DialogTurnResult> HandleSomethingMoreQuestingAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var repeatDialog = (bool)stepContext.Result;
            if (repeatDialog)
            {
                return await stepContext.BeginDialogAsync(nameof(WaterfallDialog),
                    cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
