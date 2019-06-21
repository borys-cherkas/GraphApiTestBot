using System;
using System.Threading;
using System.Threading.Tasks;
using GraphApiTestBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GraphApiTestBot.Extensions
{
    public static class OAuthHelpers
    {
        public static async Task ShowMyProfileInfoAsync(ITurnContext turnContext, TokenState tokenState, CancellationToken ct = new CancellationToken())
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenState == null)
            {
                throw new ArgumentNullException(nameof(tokenState));
            }

            // Pull in the data from the Microsoft Graph.
            var client = new SimpleGraphClient(tokenState.AccessToken);
            var me = await client.GetMeAsync(ct);

            await turnContext.SendActivityAsync($"You are {me.DisplayName}.");
        }

        public static async Task ShowFilesAsync(ITurnContext turnContext, TokenState tokenState, CancellationToken ct = new CancellationToken())
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenState == null)
            {
                throw new ArgumentNullException(nameof(tokenState));
            }

            var client = new SimpleGraphClient(tokenState.AccessToken);
            var me = await client.GetMicrosoftGraphOneDriveFilesAsync(ct);

            await turnContext.SendActivityAsync($"OneDrive items count: {me.Count}.");
        }
    }
}
