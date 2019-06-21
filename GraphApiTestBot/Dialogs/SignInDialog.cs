using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphApiTestBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace GraphApiTestBot.Dialogs
{
    public class SignInDialog : ComponentDialog
    {
        public SignInDialog(IConfiguration configuration) : base(nameof(SignInDialog))
        {
            ConnectionName = configuration["ConnectionName"];

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync,
                CommandStepAsync,
                ProcessStepAsync
            }));

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please login",
                    Title = "Login",
                    Timeout = 300000, // User has 5 minutes to login
                }));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        }

        public string ConnectionName { get; }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Would you like to do?"),
                        Choices = ChoiceFactory.ToChoices(new List<string> { "Help", "Profile info", "List OneDrive items" }),
                    }, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> CommandStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            stepContext.Values["command"] = stepContext.Result;

            // Call the prompt again because we need the token. The reasons for this are:
            // 1. If the user is already logged in we do not need to store the token locally in the bot and worry
            // about refreshing it. We can always just call the prompt again to get the token.
            // 2. We never know how long it will take a user to respond. By the time the
            // user responds the token may have expired. The user would then be prompted to login again.
            //
            // There is no reason to store the token locally in the bot because we can always just call
            // the OAuth prompt to get the token or get a new token if needed.
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (stepContext.Result != null)
            {
                // We do not need to store the token in the bot. When we need the token we can
                // send another prompt. If the token is valid the user will not need to log back in.
                // The token will be available in the Result property of the task.
                var tokenResponse = stepContext.Result as TokenResponse;

                // If we have the token use the user is authenticated so we may use it to make API calls.
                if (tokenResponse?.Token != null)
                {
                    var command = ((string)stepContext.Values["command"] ?? string.Empty).ToLowerInvariant().Trim();

                    if (command == "help")
                    {
                        // show help
                    }
                    else if (command.StartsWith("profile info"))
                    {
                        await OAuthHelpers.ListMeAsync(stepContext.Context, tokenResponse);
                    }
                    else if (command.StartsWith("list"))
                    {
                        await OAuthHelpers.ShowFilesAsync(stepContext.Context, tokenResponse);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(
                            MessageFactory.Text($"Your token is: {tokenResponse.Token}"), cancellationToken);
                    }
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("We couldn't log you in. Please try again later."), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
