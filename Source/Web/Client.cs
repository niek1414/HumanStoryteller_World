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

        public static IRestResponse Get(string url) {
            RestClient restClient = CreateRestClient();
            RestRequest request = new RestRequest(url, Method.GET);
            return restClient.ExecuteAsGet(request, "GET");
        }

        public static void GetByPaginationAndFilter(string url, Action<IRestResponse, RestRequestAsyncHandle> callback, long start, long amount,
            string filterName, string filterDescription, string filterCreator, String ticket = null) {
            RestClient restClient = CreateRestClient();
            RestRequest request = new RestRequest(url, Method.GET);
            if (ticket != null) {
                request.AddParameter("ticket", ticket);
            }

            request.AddParameter("start", start.ToString());
            request.AddParameter("amount", amount.ToString());
            request.AddParameter("name", filterName);
            request.AddParameter("description", filterDescription);
            request.AddParameter("creator", filterCreator);

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