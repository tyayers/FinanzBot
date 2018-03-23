using FinanzBot.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
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

                string requestUri = ConfigurationManager.AppSettings["QnAEndpoint"];

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
                string requestUri = ConfigurationManager.AppSettings["LuisEndpoint"] + Query;

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