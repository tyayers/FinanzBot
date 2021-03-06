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
                // Kredit
                await context.PostAsync("Wollen Sie die Kreditablösesumme jetzt anfordern?");
                context.Wait(this.StartKreditAblöseDialog);
            }
            //else if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.8 && luisInfo.intents[0].intent == "Wikipedia")
            //{
            //    // Wikipedia
            //    string answer = "Das weiß ich nicht, ich lerne noch..";
            //    string wikiResult = await ServiceProxies.SearchWikipedia(luisInfo.entities[0].entity);

            //    Newtonsoft.Json.Linq.JArray jsonResult = JArray.Parse(wikiResult);
            //    JArray titleArray = (JArray)jsonResult[1];
            //    JArray descriptionArray = (JArray)jsonResult[2];
            //    JArray linkArray = (JArray)jsonResult[3];

            //    if (titleArray.Count > 0)
            //    {
            //        answer = "Ich kenne mich nicht so aus, aber hier sind Infos von Wikipedia: " + titleArray[0].ToString() + ". " + descriptionArray[0].ToString();
            //    }

            //    await context.PostAsync(answer);
            //    context.Wait(MessageReceivedAsync);
            //}
            else
            {
                // Fall back on QnAMaker
                QnAResponse response = await ServiceProxies.GetQnAResponse(message.Text);
                string answer = "Das weiß ich nicht, ich lerne noch..";

                if (response.answers.Length > 0 && response.answers[0].score > 0.7)
                {
                    if (smalltalkData[response.answers[0].answer] != null)
                    {
                        // Get random answer from array
                        int index = randomGen.Next(0, ((JArray)smalltalkData[response.answers[0].answer]).Count - 1);
                        answer = smalltalkData[response.answers[0].answer][index].ToString();
                    }
                    else if (!response.answers[0].answer.Contains(".agent."))
                    {
                        answer = response.answers[0].answer;
                    }
                    else
                    {
                        System.Diagnostics.Trace.TraceError("MISSING-ANSWER: " + message.Text);
                    }
                }
                else
                {
                    System.Diagnostics.Trace.TraceError("MISSING-ANSWER: " + message.Text);
                }

                await context.PostAsync(answer);

                context.Wait(MessageReceivedAsync);
            }
        }

        protected virtual async Task StartKreditAblöseDialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var input = await result;

            LuisResponse luisInfo = await ServiceProxies.GetEntityFromLUIS(input.Text);

            if (luisInfo.intents.Length > 0 && luisInfo.intents[0].intent == "Ja")
            {
                // Yes start kreditablösedialog
                IDialog<string> kreditDialog = (IDialog<string>) new KreditAblöseDialog();
                context.Call(kreditDialog, AfterKreditDialog);
            }
            else
            {
                context.Wait(MessageReceivedAsync);
            }
        }

        protected virtual async Task AfterKreditDialog(IDialogContext context, IAwaitable<string> argument)
        {
            //await context.PostAsync($"Vielen Dank, Ihre Kreditablöseinformation ist unterwegs.");
            context.Wait(this.MessageReceivedAsync);
        }
    }
}