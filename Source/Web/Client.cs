using System;
using RestSharp;

namespace HumanStoryteller.Web {
    public static class Client {
        private static string _host = "http://storyteller.keyboxsoftware.nl/";

        public static void Get(string url, Action<IRestResponse, RestRequestAsyncHandle> callback, String ticket = null) {
            RestClient restClient = CreateRestClient();
            RestRequest request = new RestRequest(url, Method.GET);
            if (ticket != null) {
                request.AddQueryParameter("ticket", ticket);
            }

            restClient.ExecuteAsyncGet(request, callback, "GET");
        }
        
        public static void Put(string url, Action<IRestResponse, RestRequestAsyncHandle> callback, String ticket = null) {
            RestClient restClient = CreateRestClient();
            RestRequest request = new RestRequest(url, Method.PUT);
            if (ticket != null) {
                request.AddQueryParameter("ticket", ticket);
            }

            restClient.ExecuteAsyncGet(request, callback, "PUT");
        }

        private static RestClient CreateRestClient() {
            RestClient restClient = new RestClient(_host) {
                UserAgent = "HumanStoryteller"
            };
            return restClient;
        }
    }
}