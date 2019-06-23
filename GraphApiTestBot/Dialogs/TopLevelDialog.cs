using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GraphApiTestBot.Extensions;
using GraphApiTestBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Configuration;

namespace GraphApiTestBot.Dialogs
{
    public class TopLevelDialog : ComponentDialog
    {
        private const string HelpCommand = "help";
        private const string ProfileInfoCommand = "profile info";
        private const string ListOneDriveItemsCommand = "list onedrive items";

        private readonly List<string> SignInRequiredCommands = new List<string> { ProfileInfoCommand, ListOneDriveItemsCommand };

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

        private async Task<DialogTurnResult> OpenSignInDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var command = ((FoundChoice)stepContext.Result).Value;
            stepContext.Values["command"] = command;

            if (!SignInRequiredCommands.Contains(command))
            {
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }

            return await stepContext.BeginDialogAsync(nameof(SignInDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> HandleSignInResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var command = (string) stepContext.Values["command"];
            if (SignInRequiredCommands.Contains(command))
            {
                var accessTokenState = (TokenState)stepContext.Result;
                stepContext.Values["accessToken"] = accessTokenState;
            }

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

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

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task ShowHelpCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Help information here...", cancellationToken: cancellationToken);
        }

        private async Task LoadAndShowProfileInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenState = (TokenState)stepContext.Values["accessToken"];

            await OAuthHelpers.ShowMyProfileInfoAsync(stepContext.Context, tokenState, cancellationToken);
        }

        private async Task LoadAndShowOneDriveItemsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenState = (TokenState)stepContext.Values["accessToken"];

            await OAuthHelpers.ShowFilesAsync(stepContext.Context, tokenState, cancellationToken);
        }

        private async Task<DialogTurnResult> AskSomethingMoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Would you like to perform another operation?")
                }, cancellationToken);
        }

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
