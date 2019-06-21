// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;

namespace GraphApiTestBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        public MainDialog(IConfiguration configuration, UserState userState, ConversationState conversationState) : base(nameof(MainDialog))
        {
            AddDialog(new TopLevelDialog(configuration, userState, conversationState));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            }));
            
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(TopLevelDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Thank you for using Graph API bot. " +
                "If you want to continue working with the bot, just send any message.", cancellationToken: cancellationToken);
            
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}