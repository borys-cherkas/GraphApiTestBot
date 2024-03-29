﻿using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace GraphApiTestBot.Extensions
{
    // This class is a wrapper for the Microsoft Graph API
    // See: https://developer.microsoft.com/en-us/graph
    public class SimpleGraphClient
    {
        private readonly string _token;

        public SimpleGraphClient(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _token = token;
        }

        // Get information about the user.
        public async Task<User> GetMeAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var graphClient = GetAuthenticatedClient();
            var me = await graphClient.Me.Request().GetAsync(cancellationToken);
            return me;
        }

        public async Task<ICollection<DriveItem>> GetMicrosoftGraphOneDriveFilesAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            List<DriveItem> driveItems = new List<DriveItem>();

            var graphClient = GetAuthenticatedClient();
            var pageDriveResponse = await graphClient.Me.Drive.Root.Children
                .Request()
                .GetAsync(cancellationToken);
            driveItems.AddRange(pageDriveResponse.CurrentPage);

            while (pageDriveResponse.NextPageRequest != null)
            {
                pageDriveResponse = await pageDriveResponse.NextPageRequest.GetAsync(cancellationToken);
                driveItems.AddRange(pageDriveResponse.CurrentPage);
            } 

            return driveItems;
        }

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        private GraphServiceClient GetAuthenticatedClient()
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));
            return graphClient;
        }
    }
}
