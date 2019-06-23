using System.Threading;
using System.Threading.Tasks;
using GraphApiTestBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace GraphApiTestBot.Dialogs
{
    public class SignInDialog : ComponentDialog
    {
        public SignInDialog(IConfiguration configuration) : base(nameof(SignInDialog))
        {
            ConnectionName = configuration["ConnectionName"];

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please login",
                    Title = "Login",
                    Timeout = 300000, // User has 5 minutes to login
                    // Timeout = 20000, // User has 20 sec to login
                }));

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ShowSignInCardAsync,
                ValidateSignInResultAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        public string ConnectionName { get; }

        private async Task<DialogTurnResult> ShowSignInCardAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ValidateSignInResultAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (!string.IsNullOrEmpty(tokenResponse?.Token))
            {
                var tokenState = new TokenState(tokenResponse.Token);
                return await stepContext.EndDialogAsync(tokenState, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Login was not successful, please try again."), cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(WaterfallDialog), null, cancellationToken: cancellationToken);
        }
    }
}
