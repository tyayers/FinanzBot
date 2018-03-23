using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using FinanzBot.Dtos;
using FinanzBot.Utilities;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;

namespace FinanzBot
{
    [Serializable]
    public class EntryDialog : IDialog<object>
    {
        protected JObject smalltalkData = new JObject();
        protected System.Random randomGen = new Random();

        public EntryDialog()
        {
            // Load smalltalk data
            using (HttpClient client = new HttpClient())
            {
                HttpContext httpCon = HttpContext.Current;
                string baseUrl = httpCon.Request.Url.Scheme + "://" + httpCon.Request.Url.Authority + httpCon.Request.ApplicationPath.TrimEnd('/') + '/';
                client.BaseAddress = new System.Uri(baseUrl);

                HttpResponseMessage msg = client.GetAsync("/wwwroot/assistant/Smalltalk-DE.js").Result;

                if (msg.IsSuccessStatusCode)
                {
                    string data = msg.Content.ReadAsStringAsync().Result;
                    smalltalkData = Newtonsoft.Json.Linq.JObject.Parse(data);
                }
            }
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            // First send typing
            var typingActivity = ((Activity)message).CreateReply();
            typingActivity.Type = ActivityTypes.Typing;
            await context.PostAsync(typingActivity);

            LuisResponse luisInfo = await ServiceProxies.GetEntityFromLUIS(message.Text);

            if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.8 && luisInfo.intents[0].intent == "Kreditablösen")
            {
                await context.PostAsync("Sie wollen die Kreditablösesumme wissen?  Das geht ganz einfach. Sagen Sie jetzt ihre Versicherungsnummer vor.");
                context.Wait(this.AfterInsuranceNumberEntry);
            }
            else
            {
                // Fall back on QnAMaker
                QnAResponse response = await ServiceProxies.GetQnAResponse(message.Text);
                string answer = "Leider habe ich keine Information gefunden!  Ich lerne noch dazu, wenn ich die Antwort habe werde ich Dich kontaktieren!";

                if (response.answers.Length > 0)
                {
                    if (smalltalkData[response.answers[0].answer] != null)
                    {
                        // Get random answer from array
                        int index = randomGen.Next(0, ((JArray)smalltalkData[response.answers[0].answer]).Count - 1);
                        answer = smalltalkData[response.answers[0].answer][index].ToString();
                    }
                    else
                    {
                        answer = response.answers[0].answer;
                    }
                }

                await context.PostAsync(answer);

                context.Wait(MessageReceivedAsync);
            }
        }
        protected virtual async Task AfterInsuranceNumberEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            string insuranceNumber = message.Text;
            await context.PostAsync($"Ihre Versicherungsnummer lautet {String.Join(" ", insuranceNumber.ToCharArray())}");

            await context.PostAsync($"Bitte sagen Sie Ihre Vor und Nach Name");
            context.Wait(this.AfterNameEntry);
        }

        protected virtual async Task AfterNameEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            string name = message.Text;
            await context.PostAsync($"Bitte sagen Sie ihre Geburtsdatum");
            context.Wait(this.AfterBirthdayEntry);
        }
        protected virtual async Task AfterBirthdayEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            string birthday = message.Text;
            await context.PostAsync($"Vielen Dank! Sie werden um die Ablösesumme Ihres Fahrzeuges mit der Post notifiziert.");
            context.Wait(this.MessageReceivedAsync);
        }
    }
}