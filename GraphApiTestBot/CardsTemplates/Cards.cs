using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using Newtonsoft.Json;
using Attachment = Microsoft.Bot.Schema.Attachment;
using File = System.IO.File;

namespace GraphApiTestBot.CardsTemplates
{
    public static class Cards
    {
        public static Task<ResourceResponse> ShowActivityWithAttachmentsAsync(
            ITurnContext turnContext, 
            IList<Attachment> attachments, 
            string layout = AttachmentLayoutTypes.List, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            var reply = MessageFactory.Attachment(attachments);
            reply.AttachmentLayout = layout;

            return turnContext.SendActivityAsync(reply, cancellationToken);
        }

        public static List<Attachment> BuildHelpAttachmentList()
        {
            var helpOverviewAttachment = CreateHelpCommandOverviewAdaptiveCardAttachment();
            var listDriveFileOverviewAttachment = CreateListAllFilesCommandOverviewAdaptiveCardAttachment();
       
            return new List<Attachment>
                {helpOverviewAttachment, listDriveFileOverviewAttachment};
        }

        public static List<Attachment> BuildOneDriveAttachmentList(ICollection<DriveItem> oneDriveItems)
        {
            var attachmentList = new List<Attachment>();

            var oneDriveItemsArray = oneDriveItems.ToArray();
            for (int i = 0; i < oneDriveItems.Count; i++)
            {
                var item = oneDriveItemsArray[i];
                var attachment = CreateAttachmentFromOneDriveItem(item);
                attachmentList.Add(attachment);
            }

            return attachmentList;
        }

        private static Attachment CreateAttachmentFromOneDriveItem(DriveItem driveItem)
        {
            var buttons = new List<CardAction>
            {
                new CardAction(ActionTypes.OpenUrl, "Open", value: driveItem.WebUrl)
            };

            const string downloadUrlKey = "@microsoft.graph.downloadUrl";
            if (driveItem.AdditionalData != null && driveItem.AdditionalData.ContainsKey(downloadUrlKey))
            {
                var value = (string)driveItem.AdditionalData[downloadUrlKey];
                buttons.Add(new CardAction(ActionTypes.OpenUrl, "Download", value: value));
            }

            var card = new HeroCard
            {
                Subtitle = GetOneDriveItemDisplayType(driveItem),
                Text = driveItem.Name,
                Buttons = buttons
            };

            return card.ToAttachment();
        }

        private static string GetOneDriveItemDisplayType(DriveItem item)
        {
            if (item.Folder != null) return "Folder";
            if (item.File != null) return "File";
            return "Unknown type";
        }

        private static Attachment CreateHelpCommandOverviewAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths = { ".", nameof(CardsTemplates), "helpCommandDescriptionCard.json" };

            return CreateAdaptiveCardAttachment(paths);
        }

        private static Attachment CreateListAllFilesCommandOverviewAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths = { ".", nameof(CardsTemplates), "listAllFilesCommandDescriptionCard.json" };

            return CreateAdaptiveCardAttachment(paths);
        }

        private static Attachment CreateAdaptiveCardAttachment(string[] paths)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));
            
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson)
            };
            return adaptiveCardAttachment;
        }
    }
}
