using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureAdAuthApp.Extensions
{
    public class AzureAdAuthorizationCodeFlow
    {
        readonly string azureAdTenant;
        readonly string clientId;
        readonly string userConsentRedirectUri;
        readonly string permissionsRequested;
        readonly string clientSecret;

        public AzureAdAuthorizationCodeFlow(IConfiguration configuration)
        {
            azureAdTenant = configuration.GetValue<string>("azureAdTenant");
            clientId = configuration.GetValue<string>("clientId");
            userConsentRedirectUri = configuration.GetValue<string>("userConsentRedirectUri");
            permissionsRequested = configuration.GetValue<string>("permissionsRequested");
            clientSecret = configuration.GetValue<string>("clientSecret");

            Console.WriteLine($"Provide user consent at: https://login.microsoftonline.com/{azureAdTenant}/oauth2/v2.0/authorize?client_id={clientId}&scope={permissionsRequested}&response_type=code&response_mode=query&redirect_uri={userConsentRedirectUri}&state=12345");
        }

        public async Task UserConsented(string code, string state)
        {
            var httpClient = new HttpClient();

            // get an access token from azure ad
            var accessToken = await httpClient.GetAzureAdToken(azureAdTenant, code, clientId, userConsentRedirectUri, clientSecret, permissionsRequested);

            var oneDriveItems = await MicrosoftGraphExtensions.GetMicrosoftGraphFindOneDriveItems(accessToken);
            foreach (var driveItem in oneDriveItems)
            {
                Console.WriteLine($"One drive item named '{driveItem.Name}' with parent '{driveItem.ParentReference.Name}'");
            }
        }
    }
}
