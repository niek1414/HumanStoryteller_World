using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RestSharp;

namespace HumanStoryteller.Web {
    public static class Client {
        private static object Lock = new object();
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
                    FileName = OSUtil.GetCurlCommandLocation(),
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                p = Process.Start(psi);
                var resultString = string.Copy(p.StandardOutput.ReadToEnd());
                var statusCode = (HttpStatusCode) Enum.Parse(typeof(HttpStatusCode), resultString.Substring(resultString.Length - 3));
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

        public static void DownloadFile(string path, string url) {
            if (File.Exists(path)) {
                Tell.Log("Retrieved from file cache (" + path + ")");
            } else {
                lock (Lock) {
                    if (File.Exists(path + ".lock")) {
                        Tell.Log("Already a lockfile (" + path + ")");
                        return;
                    }

                    using (FileStream fs = File.Create(path + ".lock")) {
                        //Reserve 
                    }
                }

                Tell.Log("Downloading file from: " + url);
                if (true) {
                    DownloadFileUsingCurl(path, url);
#pragma warning disable 162
                    // ReSharper disable once HeuristicUnreachableCode
                } else {
                    // ReSharper disable once HeuristicUnreachableCode
                    DownloadFileUsingMono(path, url); //maybe usable in RimWorld version 1.1
                }
#pragma warning restore 162
            }
        }

        private static void DownloadFileUsingMono(string path, string url) {
            RestClient client = new RestClient(url) {
                UserAgent = "HumanStoryteller",
                FollowRedirects = true
            };

            var response = client.Execute(new RestRequest());

            if (!File.Exists(path) && File.Exists(path + ".lock")) {
                using (FileStream fs = File.Create(path)) {
                    fs.Write(response.RawBytes, 0, response.RawBytes.Length);
                }

                File.Delete(path + ".lock");
            } else {
                Tell.Log("Downloaded file twice");
            }
        }

        private static void DownloadFileUsingCurl(string path, string url) {
            Process p = null;

            try {
                var arguments = $"-k {url} " +
                                "-X GET " +
                                "-w \"%{http_code}\" " +
                                "-s " +
                                $"-o {path}";
                var psi = new ProcessStartInfo {
                    FileName = OSUtil.GetCurlCommandLocation(),
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                p = Process.Start(psi);
                var resultString = string.Copy(p.StandardOutput.ReadToEnd());
                var statusCode = (HttpStatusCode) Enum.Parse(typeof(HttpStatusCode), resultString);
                var intCode = (int) statusCode;
                Tell.Debug($"HTTP File download {url} ({statusCode})");
                if (intCode < 199 || intCode > 299) {
                    Tell.Warn("Unable to download file: " + statusCode);
                }
            } catch (Exception e) {
                Tell.Err($"Error while downloading: {url} to {path}", e);
            } finally {
                if (p != null && p.HasExited == false)
                    try {
                        p.Kill();
                    } catch (InvalidOperationException) {
                        //Ignore
                    }

                File.Delete(path + ".lock");
            }
        }
    }
}