using System.Linq;
using Microsoft.Bot.Schema;

namespace GraphApiTestBot.Extensions
{
    public static class ActivityExtensions
    {
        public static bool UserHasJustSentMessage(this Activity activity)
        {
            return activity.Type == ActivityTypes.Message;
        }

        public static bool UserHasJustJoinedConversation(this Activity activity)
        {
            return activity.Type == ActivityTypes.ConversationUpdate && activity.MembersAdded.FirstOrDefault()?.Id != activity.Recipient.Id;
        }
    }
}
