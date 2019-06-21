using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        public TopLevelDialog(IConfiguration configuration, UserState userState, ConversationState conversationState) 
            : base(nameof(TopLevelDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                OpenActionChooserAsync,
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
                    Choices = ChoiceFactory.ToChoices(new List<string> { HelpCommand, ProfileInfoCommand, ListOneDriveItemsCommand }),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ExecuteChosenActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = ((FoundChoice)stepContext.Result).Value;

            switch (message.ToLower().Trim())
            {
                case HelpCommand:
                {
                    // TODO: Create and show Help card here 
                    await stepContext.Context.SendActivityAsync("Help information here...", cancellationToken: cancellationToken);
                    break;
                }
                case ProfileInfoCommand:
                {
                    // TODO: Create and show profile info card here 
                    await stepContext.Context.SendActivityAsync("Profile info here...", cancellationToken: cancellationToken);
                    break;
                }
                case ListOneDriveItemsCommand:
                {
                    // TODO: Create and show list one drive files card here 
                    await stepContext.Context.SendActivityAsync("One drive files here...", cancellationToken: cancellationToken);
                    break;
                }
                default:
                {
                    await stepContext.Context.SendActivityAsync("Unknown command...", cancellationToken: cancellationToken);
                    break;
                }
            }

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
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
