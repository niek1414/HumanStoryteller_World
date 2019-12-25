using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RestSharp;

namespace HumanStoryteller.Web {
    public static class Client {
        private static string _host = "https://modstoryteller.keyboxsoftware.nl/";

        public static void Get(string url, Action<IRestResponse, RestRequestAsyncHandle> callback, String ticket = null) {
//            RestClient restClient = CreateRestClient();
//            RestRequest request = new RestRequest(url, Method.GET);
//            if (ticket != null) {
//                request.AddQueryParameter("ticket", ticket);
//            }
//
//            restClient.ExecuteAsyncGet(request, callback, "GET");
            callback(Curl("GET", _host + url, "", new Dictionary<string, string>(), ticket), null);
        }

        public static IRestResponse Get(string url) {
//            RestClient restClient = CreateRestClient();
//            RestRequest request = new RestRequest(url, Method.GET);
//            return restClient.ExecuteAsGet(request, "GET");
            return Curl("GET", _host + url, "", new Dictionary<string, string>(), null);
        }

        public static void GetByPaginationAndFilter(string url, Action<IRestResponse, RestRequestAsyncHandle> callback, long start, long amount,
            string filterName, string filterDescription, string filterCreator, String ticket = null) {
//            RestClient restClient = CreateRestClient();
//            RestRequest request = new RestRequest(url, Method.GET);
//            if (ticket != null) {
//                request.AddParameter("ticket", ticket);
//            }
//
//            request.AddParameter("start", start.ToString());
//            request.AddParameter("amount", amount.ToString());
//            request.AddParameter("name", filterName);
//            request.AddParameter("description", filterDescription);
//            request.AddParameter("creator", filterCreator);
//
//            restClient.ExecuteAsyncGet(request, callback, "GET");
            callback(Curl("GET", _host + url, "", new Dictionary<string, string> {
                {"start", start.ToString()},
                {"amount", amount.ToString()},
                {"name", filterName},
                {"description", filterDescription},
                {"creator", filterCreator}
            }, ticket), null);
        }

        public static void Put(string url, Action<IRestResponse, RestRequestAsyncHandle> callback, String ticket = null) {
//            RestClient restClient = CreateRestClient();
//            RestRequest request = new RestRequest(url, Method.PUT);
//            if (ticket != null) {
//                request.AddQueryParameter("ticket", ticket);
//            }
//
//            restClient.ExecuteAsyncGet(request, callback, "PUT");
            callback(Curl("PUT", _host + url, "", new Dictionary<string, string>(), ticket), null);
        }

        private static RestClient CreateRestClient() {
            RestClient restClient = new RestClient(_host) {
                UserAgent = "HumanStoryteller"
            };
            return restClient;
        }

        /**
         * Stupid old MONO version....
         */
        private static IRestResponse Curl(string method, string url, string data, Dictionary<string, string> parms, String ticket) {
            Process p = null;
            bool firstParam = true;
            url += "?";
            if (ticket != null) {
                firstParam = false;
                url += $"ticket={ticket}";
            }

            foreach (var parm in parms) {
                if (!firstParam) {
                    url += "&";
                }

                firstParam = false;
                url += $"{parm.Key}={parm.Value}";
            }

            try {
                var arguments = $"-k {url} " +
                                $"-X {method} " +
                                (data != "" ? $"--data \"{data}\"" : "") +
                                "-w \"%{http_code}\" " +
                                "-s";
                var psi = new ProcessStartInfo {
                    FileName = "curl",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                p = Process.Start(psi);
                var resultString = string.Copy(p.StandardOutput.ReadToEnd());
                var statusCode = (HttpStatusCode) Enum.Parse(typeof (HttpStatusCode), resultString.Substring(resultString.Length - 3));
                var content = resultString.Substring(0, resultString.Length - 3);
                Tell.Debug($"HTTP Request {url} ({statusCode}) \n{content}");
                return new RestResponse {
                    ResponseStatus = ResponseStatus.Completed, StatusCode = statusCode, Content = content
                };
            } finally {
                if (p != null && p.HasExited == false)
                    try {
                        p.Kill();
                    } catch (InvalidOperationException) {
                        //Ignore
                    }
            }
        }
    }
}