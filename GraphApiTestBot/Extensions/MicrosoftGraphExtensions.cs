using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace GraphApiTestBot.Extensions
{
    public static class MicrosoftGraphExtensions
    {
        public static async Task<IDriveItemChildrenCollectionPage> GetMicrosoftGraphOneDriveFilesAsync(string accessToken)
        {
            var graphClient = new GraphServiceClient(new PreAuthorizedBearerTokenAuthenticationProvider(accessToken));

            var driveResponse = await graphClient.Me.Drive.Root.Children
                .Request()
                .GetAsync();

            return driveResponse;
        }

        private class PreAuthorizedBearerTokenAuthenticationProvider : IAuthenticationProvider
        {
            public PreAuthorizedBearerTokenAuthenticationProvider(string accessToken)
            {
                AccessToken = accessToken;
            }

            private string AccessToken { get; }

            public async Task AuthenticateRequestAsync(HttpRequestMessage request)
            {
                request.Headers.Add("Authorization", $"Bearer {AccessToken}");
            }
        }
    }
}
