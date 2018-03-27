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
    public class KreditAblöseDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Bitte sagen Sie jetzt Ihre Versicherungsnummer laut vor.");
            context.Wait(AfterInsuranceNumberEntry);
        }

        protected virtual async Task AfterInsuranceNumberEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            LuisResponse luisInfo = await ServiceProxies.GetEntityFromLUIS(message.Text);
            if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.8 && luisInfo.intents[0].intent == "Abort")
            {
                await context.PostAsync("Ok, sorry, wir hören jetzt auf. Vielleicht ein anderes mal!");
                context.Done<string>(null);
            }
            else
            {
                string insuranceNumber = message.Text;
                await context.PostAsync($"Ihre Versicherungsnummer lautet {String.Join(" ", insuranceNumber.ToCharArray())}. Ist das richtig?");
                context.Wait(this.ConfirmInsuranceNumberEntry);
            }
        }

        protected virtual async Task ConfirmInsuranceNumberEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            LuisResponse luisInfo = await ServiceProxies.GetEntityFromLUIS(message.Text);

            if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.8 && luisInfo.intents[0].intent == "Ja")
            {
                await context.PostAsync($"Bitte sagen Sie Ihren Vor und Nach Namen");
                context.Wait(this.AfterNameEntry);
            }
            else if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.8 && luisInfo.intents[0].intent == "Abort")
            {
                await context.PostAsync("Ok, sorry, wir hören jetzt auf. Vielleicht ein anderes mal!");
                context.Done<string>(null);
            }
            else
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }

        protected virtual async Task AfterNameEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            LuisResponse luisInfo = await ServiceProxies.GetEntityFromLUIS(message.Text);
            if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.8 && luisInfo.intents[0].intent == "Abort")
            {
                await context.PostAsync("Ok, sorry, wir hören jetzt auf. Vielleicht ein anderes mal!");
                context.Done<string>(null);
            }
            else
            {
                string name = message.Text;
                await context.PostAsync($"Bitte sagen Sie Ihr Geburtsdatum");
                context.Wait(this.AfterBirthdayEntry);
            }
        }
        protected virtual async Task AfterBirthdayEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            LuisResponse luisInfo = await ServiceProxies.GetEntityFromLUIS(message.Text);
            if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.8 && luisInfo.intents[0].intent == "Abort")
            {
                await context.PostAsync("Ok, sorry, wir hören jetzt auf. Vielleicht ein anderes mal!");
                context.Done<string>(null);
            }
            else
            {
                string birthday = message.Text;
                await context.PostAsync($"Vielen Dank! Sie werden um die Ablösesumme Ihres Fahrzeuges mit der Post Ihnen schnellstmöglich zusenden.");
                context.Done<string>(null);
            }
        }
    }
}