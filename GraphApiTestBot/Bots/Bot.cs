// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Schema;

namespace GraphApiTestBot.Bots
{
    public class Bot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"Borys CD Echo: {turnContext.Activity.Text}"), cancellationToken);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            var teamsContext = turnContext.TurnState.Get<ITeamsContext>();

            if (teamsContext == null) return;

            // Now fetch the Team ID, Channel ID, and Tenant ID off of the incoming activity
            var incomingTeamId = teamsContext.Team.Id;
            var incomingChannelid = teamsContext.Channel.Id;
            var incomingTenantId = teamsContext.Tenant.Id;

            // Make an operation call to fetch the list of channels in the team, and print count of channels.
            var channels = await teamsContext.Operations.FetchChannelListAsync(incomingTeamId);
            await turnContext.SendActivityAsync($"You have {channels.Conversations.Count} channels in this team");

            // Make an operation call to fetch details of the team where the activity was posted, and print it.
            var teamInfo = await teamsContext.Operations.FetchTeamDetailsAsync(incomingTeamId);
            await turnContext.SendActivityAsync($"Name of this team is {teamInfo.Name} and group-id is {teamInfo.AadGroupId}");
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
    }
}
