using FinanzBot.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
                client.Headers.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["QnAKey"]);
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

        public static async Task<string> SearchWikipedia(string message)
        {
            string result = "";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://de.wikipedia.org/w/api.php?action=opensearch&format=json&search=" + message);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync("");
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
    }
}