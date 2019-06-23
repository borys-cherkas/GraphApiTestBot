using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using static Microsoft.Bot.Builder.Dialogs.Choices.Channel;

namespace GraphApiTestBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly Dialog Dialog;
        protected readonly ILogger Logger;
        protected readonly BotState UserState;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext?.Activity?.Type == ActivityTypes.Invoke && turnContext.Activity.ChannelId == Channels.Msteams)
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            else
                await base.OnTurnAsync(turnContext, cancellationToken);

            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Token Response Event Activity.");

            // Run the Dialog with the new Token Response Event Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
    }
}
