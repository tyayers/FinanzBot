using FinanzBot.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace FinanzBot.Utilities
{
    public static class ServiceProxies
    {
        public static async Task<QnAResponse> GetQnAResponse(string Question)
        {
            QnARequest request = new QnARequest();
            request.question = Question;

            QnAResponse data = new QnAResponse();

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Ocp-Apim-Subscription-Key", "6a50acd8314e456191ecf4cbf40164c9");
                client.Encoding = System.Text.Encoding.UTF8;

                string requestUri = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/635ac944-0b31-4346-aed6-b6668a8fd452/generateAnswer";

                string responseString = client.UploadString(requestUri, JsonConvert.SerializeObject(request));
                string decodedString = System.Net.WebUtility.HtmlDecode(responseString).Replace("\"", "'").Replace("„", "'");
                data = JsonConvert.DeserializeObject<QnAResponse>(decodedString);
            }

            return data;
        }

        public static async Task<LuisResponse> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            LuisResponse data = new LuisResponse();
            using (HttpClient client = new HttpClient())
            {
                //string requestUri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/58a3a568-120d-4bd7-84bc-fe2c58e42894?subscription-key=164a83ce971642bdb3663420627a5c73&verbose=true&timezoneOffset=0&q=" + Query;
                string requestUri = "https://eastus2.api.cognitive.microsoft.com/luis/v2.0/apps/05426d0a-66ff-407d-a80e-c274af62a987?subscription-key=049da8775cd147ddb9f167674d02f5b6&verbose=true&timezoneOffset=0&q=" + Query;

                HttpResponseMessage msg = await client.GetAsync(requestUri);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    data = JsonConvert.DeserializeObject<LuisResponse>(JsonDataResponse);
                }
            }

            return data;
        }
    }
}