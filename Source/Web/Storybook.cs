using System;
using System.Net;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RestSharp;

namespace HumanStoryteller.Web {
    public static class Storybook {
        public static void GetStory(long id, Action<Story> callback) {
            Tell.Log($"AutoPoll on story: {id}");
            Client.Get($"storybook/story/{id}", (response, handle) => { GetStoryCallback(response, handle, callback); });
        }

        private static void GetStoryCallback(IRestResponse response, RestRequestAsyncHandle handle, Action<Story> callback) {
            try {
                if (response.StatusCode == HttpStatusCode.NotFound) {
                    Tell.Warn("Story does not exist on the server (anymore).");
                    callback(null);
                    return;
                }

                if (!CheckRequestStatus(response, handle)) {
                    return;
                }

                callback(Parser.Parser.StoryParser(response.Content));
            } catch (Exception e) {
                Tell.Err($"Error while parsing story with error: {e.Message}, and content request: ", response);
                throw;
            }
        }


        public static void GetBook(Action<StorySummary[]> getSummariesCallback) {
            //TODO pagination
            Client.Get("storybook/story", (response, handle) => { GetSummariesCallback(response, handle, getSummariesCallback); });
        }

        private static void GetSummariesCallback(IRestResponse response, RestRequestAsyncHandle handle, Action<StorySummary[]> callback) {
            if (!CheckRequestStatus(response, handle)) {
                return;
            }

            callback(Parser.Parser.SummaryParser(response.Content));
        }

        public static void GetCollectionOfCreator(long id, Action<StorySummary[]> getSummariesCallback) {
            Client.Get($"storybook/story/creator/{id}", (response, handle) => { GetSummariesCallback(response, handle, getSummariesCallback); });
        }

        private static void GetRatingCallback(IRestResponse response, RestRequestAsyncHandle handle, Action<int> callback) {
            if (!CheckRequestStatus(response, handle)) {
                return;
            }

            callback(Convert.ToInt32(response.Content));
        }

        public static void GetRating(long storyId, Action<int> getRatingCallback) {
            SteamAuth.GetEncodedTicket(ticket => { AfterEncode(storyId, getRatingCallback, ticket); });

            void AfterEncode(long l, Action<int> action, string ticket) {
                Client.Get(
                    $"storybook/story/{l}/rating",
                    (response, handle) => { GetRatingCallback(response, handle, action); },
                    ticket);
            }
        }
        
        private static void SetRatingCallback(IRestResponse response, RestRequestAsyncHandle handle, Action callback) {
            if (!CheckRequestStatus(response, handle)) {
                return;
            }

            callback();
        }
        
        public static void SetRating(long storyId, int rating, Action setRatingCallback) {
            SteamAuth.GetEncodedTicket(ticket => { AfterEncodePut(storyId, rating, ticket); });

            void AfterEncodePut(long l, int i, string ticket) {
                i--;
                Client.Put(
                    $"storybook/story/{l}/rating/{i}",
                    (response, handle) => { SetRatingCallback(response, handle, setRatingCallback); },
                    ticket);
            }
        }

        private static bool CheckRequestStatus(IRestResponse response, RestRequestAsyncHandle handle) {
            if (response.ResponseStatus != ResponseStatus.Completed) {
                Tell.Warn($"Rest call failed, {response.ResponseStatus}", response, handle);
                return false;
            }

            if (response.StatusCode != HttpStatusCode.OK) {
                Tell.Warn($"Rest call failed, {response.StatusCode}", response, handle);
                return false;
            }

            return true;
        }
    }
}