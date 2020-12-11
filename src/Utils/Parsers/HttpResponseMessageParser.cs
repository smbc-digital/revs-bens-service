using System;
using System.Net.Http;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways.Response;

namespace revs_bens_service.Utils.Parsers
{
    public static class HttpResponseMessageParser
    {
        public static HttpResponse<T> Parse<T>(this HttpResponseMessage responseMessage)
        {
            T deserializedObject;

            try
            {
                var content = responseMessage.Content.ReadAsStringAsync().Result;
                deserializedObject = JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception)
            {
                deserializedObject = default(T);
            }

            return new HttpResponse<T>
            {
                ResponseContent = deserializedObject,
                Headers = responseMessage.Headers,
                IsSuccessStatusCode = responseMessage.IsSuccessStatusCode,
                ReasonPhrase = responseMessage.ReasonPhrase,
                StatusCode = responseMessage.StatusCode,
                Version = responseMessage.Version
            };
        }
    }
}