using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GraphApiTestBot.Extensions;
using GraphApiTestBot.State;
using Microsoft.Graph;

namespace GraphApiTestBot.Graph
{
    public static class GraphApiHelper
    {
        public static async Task<User> GetCurrentUserAsync(TokenState tokenState, CancellationToken ct = new CancellationToken())
        {
            if (tokenState == null)
            {
                throw new ArgumentNullException(nameof(tokenState));
            }
            
            var client = new SimpleGraphClient(tokenState.AccessToken);
            var me = await client.GetMeAsync(ct);

            return me;
        }

        public static async Task<ICollection<DriveItem>> GetOneDriveFilesListAsync(TokenState tokenState, CancellationToken ct = new CancellationToken())
        {
            if (tokenState == null)
            {
                throw new ArgumentNullException(nameof(tokenState));
            }

            var client = new SimpleGraphClient(tokenState.AccessToken);
            var oneDriveItems = await client.GetMicrosoftGraphOneDriveFilesAsync(ct);

            return oneDriveItems;
        }
    }
}
