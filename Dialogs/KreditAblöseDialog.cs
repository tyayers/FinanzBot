using FinanzBot.Dtos;
using FinanzBot.Utilities;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FinanzBot
{
    [Serializable]
    public class KreditAblöseDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await MessageReceivedAsync(context, null);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            await context.PostAsync("Bitte sagen Sie jetzt Ihre Versicherungsnummer laut vor.");
            context.Wait(AfterInsuranceNumberEntry);
        }

        protected virtual async Task AfterInsuranceNumberEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            LuisResponse luisInfo = await ServiceProxies.GetEntityFromLUIS(message.Text);
            if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.6 && luisInfo.intents[0].intent == "Abort")
            {
                await context.PostAsync("Ok, sorry, wir hören jetzt auf. Vielleicht ein anderes mal!");
                context.Done<string>(null);
            }
            else
            {
                string insuranceNumber = message.Text;
                context.ConversationData.SetValue<string>("INSURANCE-NUMBER", insuranceNumber);
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
                await context.PostAsync($"Bitte sagen Sie jetzt Ihren Vor und Nach Namen laut vor");
                context.Wait(this.AfterNameEntry);
            }
            else if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.6 && luisInfo.intents[0].intent == "Abort")
            {
                await context.PostAsync("Ok, sorry, wir hören jetzt auf. Vielleicht ein anderes mal!");
                context.Done<string>(null);
            }
            else
            {
                await MessageReceivedAsync(context, result);
            }
        }

        protected virtual async Task AfterNameEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            context.ConversationData.SetValue<string>("INSURANCE-NAME", message.Text);
            LuisResponse luisInfo = await ServiceProxies.GetEntityFromLUIS(message.Text);
            if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.6 && luisInfo.intents[0].intent == "Abort")
            {
                await context.PostAsync("Ok, sorry, wir hören jetzt auf. Vielleicht ein anderes mal!");
                context.Done<string>(null);
            }
            else
            {
                string name = message.Text;
                await context.PostAsync($"Bitte sagen Sie jetzt Ihr Geburtsdatum laut vor");
                context.Wait(this.AfterBirthdayEntry);
            }
        }
        protected virtual async Task AfterBirthdayEntry(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            context.ConversationData.SetValue<string>("INSURANCE-BIRTHDAY", message.Text);

            LuisResponse luisInfo = await ServiceProxies.GetEntityFromLUIS(message.Text);
            if (luisInfo.intents != null && luisInfo.intents.Length > 0 && luisInfo.intents[0].score > 0.6 && luisInfo.intents[0].intent == "Abort")
            {
                await context.PostAsync("Ok, sorry, wir hören jetzt auf. Vielleicht ein anderes mal!");
                context.Done<string>(null);
            }
            else
            {
                string birthday = message.Text;
                string insuranceNumber = context.ConversationData.GetValue<string>("INSURANCE-NUMBER");
                string insuranceName = context.ConversationData.GetValue<string>("INSURANCE-NAME");
                System.Diagnostics.Trace.TraceInformation($"KREDITABL {insuranceNumber} - {insuranceName} - {birthday}");
                await context.PostAsync($"Vielen Dank, Ihre Kreditablöseinformation ist unterwegs!");
                context.Done<string>(null);
            }
        }
    }
}