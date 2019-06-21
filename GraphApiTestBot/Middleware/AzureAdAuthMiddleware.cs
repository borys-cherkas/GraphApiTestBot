// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GraphApiTestBot.Extensions;
using GraphApiTestBot.Storage;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace GraphApiTestBot.Middleware
{
    public class AzureAdAuthMiddleware : IMiddleware
    {
        private const string AuthTokenKey = "authToken";
        
        public AzureAdAuthMiddleware(IAuthTokenStorage tokenStorage, IConfiguration configuration)
        {
            TokenStorage = tokenStorage;
            AzureAdTenant = configuration.GetValue<string>(nameof(AzureAdTenant));
            AppClientId = configuration.GetValue<string>(nameof(AppClientId));
            AppRedirectUri = configuration.GetValue<string>(nameof(AppRedirectUri));
            AppClientSecret = configuration.GetValue<string>(nameof(AppClientSecret));
            PermissionsRequested = configuration.GetValue<string>(nameof(PermissionsRequested));
        }

        public IAuthTokenStorage TokenStorage { get; }

        public string AzureAdTenant { get; }

        public string AppClientId { get; }

        public string AppRedirectUri { get; }

        public string AppClientSecret { get; }

        public string PermissionsRequested { get; }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var authToken = TokenStorage.LoadConfiguration(context.Activity.Conversation.Id);

            if (authToken == null)
            {
                if (context.Activity.UserHasJustSentMessage() || context.Activity.UserHasJustJoinedConversation())
                {
                    var conversationReference = context.Activity.GetConversationReference();

                    var serializedCookie = WebUtility.UrlEncode(JsonConvert.SerializeObject(conversationReference));

                    var signInUrl = AzureAdExtensions.GetUserConsentLoginUrl(AzureAdTenant, AppClientId, AppRedirectUri, PermissionsRequested, serializedCookie);

                    var activity = context.Activity.CreateReply();
                    activity.AddSignInCard(signInUrl);

                    await context.SendActivityAsync(activity, cancellationToken);
                }
            }
            else if (authToken.ExpiresIn < DateTime.Now.AddMinutes(-10))
            {
                if (context.Activity.UserHasJustSentMessage() || context.Activity.UserHasJustJoinedConversation())
                {
                    using (var client = new HttpClient())
                    {
                        var accessToken = await client.GetAccessTokenUsingRefreshTokenAsync(AzureAdTenant,
                            authToken.RefreshToken, AppClientId, AppRedirectUri, AppClientSecret, PermissionsRequested);

                        authToken = new ConversationAuthToken(context.Activity.Conversation.Id)
                        {
                            AccessToken = accessToken.accessToken,
                            RefreshToken = accessToken.refreshToken,
                            ExpiresIn = accessToken.refreshTokenExpiresIn
                        };
                        TokenStorage.SaveConfiguration(authToken);

                        context.TurnState.Add(AuthTokenKey, authToken);
                        await next(cancellationToken);
                    }
                }
            }
            else
            {
                context.TurnState.Add(AuthTokenKey, authToken);
                await next(cancellationToken);
            }
        }
    }
}