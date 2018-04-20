using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using System.Web;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FinanzBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // check if activity is of type message
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new EntryDialog().DefaultIfException());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                var client = new ConnectorClient(new Uri(message.ServiceUrl));
                IConversationUpdateActivity update = message;
                if (update.MembersAdded.Any())
                {
                    var reply = message.CreateReply();
                    var newMembers = update.MembersAdded?.Where(t => t.Id != message.Recipient.Id);
                    foreach (var newMember in newMembers)
                    {
                        // Hero card way
                        //var reply = message.CreateReply();
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: "https://docsearch.blob.core.windows.net/files/fin-advisor.jpg"));

                        List<CardAction> cardButtons = new List<CardAction>();

                        CardAction plButtonLoan = new CardAction()
                        {
                            Value = "Finanzierung",
                            Type = "imBack",
                            Title = "Persönlichisierte Finanzierung"
                        };
                        cardButtons.Add(plButtonLoan);

                        CardAction plButtonCredit = new CardAction()
                        {
                            Value = "Dispotkredit Erhöhen",
                            Type = "imBack",
                            Title = "Dispotkrediterhöhung"
                        };
                        cardButtons.Add(plButtonCredit);

                        CardAction plCloseCreditButton = new CardAction()
                        {
                            Value = "Ablösesumme",
                            Type = "imBack",
                            Title = "Kreditablösesumme Anfragen"
                        };
                        cardButtons.Add(plCloseCreditButton);

                        HeroCard plCard = new HeroCard()
                        {
                            Title = $"Hallo, ich bin Pia!",
                            Subtitle = $"Ich kann Fragen zu Deinen Konten und Finanzierungen beantworten, zum Beispiel:",
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        reply.Attachments.Add(plAttachment);


                        // User suggestions way
                        //reply.Type = ActivityTypes.Message;
                        //reply.TextFormat = TextFormatTypes.Plain;

                        //reply.SuggestedActions = new SuggestedActions()
                        //{
                        //    Actions = new List<CardAction>()
                        //    {
                        //        new CardAction(){ Title = "Blue", Type=ActionTypes.ImBack, Value="Blue" },
                        //        new CardAction(){ Title = "Red", Type=ActionTypes.ImBack, Value="Red" },
                        //        new CardAction(){ Title = "Green", Type=ActionTypes.ImBack, Value="Green" }
                        //    }
                        //};

                        client.Conversations.ReplyToActivityAsync(reply);
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}