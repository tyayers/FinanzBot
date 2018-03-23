using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using FinanzBot.Dtos;
using FinanzBot.Utilities;

namespace FinanzBot
{
    [Serializable]
    public class EntryDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

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

                //if (response.answers != null && response.answers.Length > 0 && response.answers[0].score > 50)
                //    answer = response.answers[0].answer;

                //if (userData.LanguageCode != "de")
                //    answer = await ServiceProxies.TranslateText(answer, "de", userData.LanguageCode);

                await context.PostAsync(answer);

                context.Wait(MessageReceivedAsync);
            }
        }
        protected virtual async Task AfterInsuranceNumberEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            string insuranceNumber = message.Text;
            await context.PostAsync($"Ihre Versicherungsnummer lautet {insuranceNumber}");

            await context.PostAsync($"Bitte sagen Sie Ihre Vor und Nach Name");
            context.Wait(this.AfterInsuranceNumberEntry);
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