using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Linq;
namespace RescueMe
{
    public class RestClient
    {
        HttpClient _httpClient;
        public RestClient(string baseUrl)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept
                   .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private StringContent GetPayload(object parameters)
        {
            var json = JsonConvert.SerializeObject(parameters);
            JObject jsonObject = JObject.FromObject(parameters);


            return new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetQueryString(object model)
        {
            if (model == null)
            {
                return "";
            }
            else
            {
                var queryString = "&" +
                    string.Join("&", model.GetType().GetRuntimeProperties().Select(p => p.Name + "=" + p.GetValue(model, null).ToString()));

                return queryString;
            }
        }

        public async Task<object> Get(string endpoint, object parameters)
        {
            var queryString = GetQueryString(parameters);
            var response =  _httpClient.GetAsync(endpoint + "?" + queryString).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error making request, more detail inner exception", new Exception(content));
            }

            return content;
        }

        public async Task<object> Post(string endpoint, object parameters)
        {
            var body = GetPayload(parameters);
            var response = _httpClient.PostAsync(endpoint, body).Result;
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error making request, more detail inner exception", new Exception(content));
            }

            return content;
        }
    }
}

