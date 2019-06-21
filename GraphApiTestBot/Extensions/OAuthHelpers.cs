using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GraphApiTestBot.Extensions
{
    // This class calls the Microsoft Graph API. The following OAuth scopes are used:
    // 'OpenId' 'email' 'Mail.Send.Shared' 'Mail.Read' 'profile' 'User.Read' 'User.ReadBasic.All'
    // for more information about scopes see:
    // https://developer.microsoft.com/en-us/graph/docs/concepts/permissions_reference
    public static class OAuthHelpers
    {

        // Displays information about the user in the bot.
        public static async Task ListMeAsync(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            // Pull in the data from the Microsoft Graph.
            var client = new SimpleGraphClient(tokenResponse.Token);
            var me = await client.GetMeAsync();

            await turnContext.SendActivityAsync($"You are {me.DisplayName}.");
        }

        public static async Task ShowFilesAsync(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            // Pull in the data from the Microsoft Graph.
            var client = new SimpleGraphClient(tokenResponse.Token);
            var me = await client.GetMicrosoftGraphOneDriveFilesAsync();

            await turnContext.SendActivityAsync($"OneDrive items count: {me.Count}.");
        }
    }
}
