using System;
using System.Net;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RestSharp;

namespace HumanStoryteller.Web; 
public static class Storybook {
    public static void GetStory(long id, Action<StoryArc> callback) {
        Tell.Log($"AutoPoll on story: [{id}]");
        if (id == -1) {
            Tell.Warn("Ether the id was unknown or this is a local story");
            return;
        }
        Client.Get($"storybook/story/{id}", (response, handle) => { GetStoryCallback(response, handle, callback); });
    } 
    
    public static StoryArc GetStory(long id) {
        Tell.Log($"Synchronous poll on story: [{id}]");
        if (id == -1) {
            Tell.Warn("Ether the id was unknown or this is a local story");
            return null;
        }
        IRestResponse response = Client.Get($"storybook/story/{id}");
        try {
            if (response.StatusCode == HttpStatusCode.NotFound) {
                Tell.Warn("Story does not exist on the server (anymore).");
                return null;
            }

            if (!CheckRequestStatus(response)) {
                return null;
            }

            return Parser.Parser.StoryParser(response.Content);
        } catch (Exception e) {
            Tell.Err($"Error while parsing story with error: [{e.Message}], trace: ___{e.StackTrace}___, and content request: ", response);
            throw;
        }
    }

    private static void GetStoryCallback(IRestResponse response, RestRequestAsyncHandle handle, Action<StoryArc> callback) {
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
            Tell.Err($"Error while parsing story with error: [{e.Message}], trace: ___{e.StackTrace}___, and content request: ", response);
            throw;
        }
    }

    public static void GetBookRandom(Action<StorySummary[]> getSummariesCallback) {
        Client.GetByPaginationAndFilter("storybook/story/random",
            (response, handle) => { GetSummariesCallback(response, handle, getSummariesCallback); },
            0,
            0,
            "",
            "",
            "");
    }
    
    public static void GetBookNew(Action<StorySummary[]> getSummariesCallback) {
        Client.GetByPaginationAndFilter("storybook/story/new",
            (response, handle) => { GetSummariesCallback(response, handle, getSummariesCallback); },
            0,
            0,
            "",
            "",
            "");
    }

    public static void GetBook(long start, long amount, string filterName, string filterDescription, string filterCreator,
        Action<StorySummary[]> getSummariesCallback) {
        Client.GetByPaginationAndFilter("storybook/story",
            (response, handle) => { GetSummariesCallback(response, handle, getSummariesCallback); }, start,
            amount, filterName, filterDescription, filterCreator);
    }

    private static void GetSummariesCallback(IRestResponse response, RestRequestAsyncHandle handle, Action<StorySummary[]> callback) {
        if (!CheckRequestStatus(response, handle)) {
            return;
        }

        try {
            callback(Parser.Parser.SummaryParser(response.Content));
        } catch (Exception e) {
            Tell.Err($"Error while parsing summary with error: [{e.Message}], trace: ___{e.StackTrace}___, and content request: ", response);
            throw;
        }
    }

    public static void GetCollectionOfCreator(long id, Action<StorySummary[]> getSummariesCallback) {
        Client.Get($"storybook/story/creator/{id}", (response, handle) => { GetSummariesCallback(response, handle, getSummariesCallback); });
    }

    private static void GetRatingCallback(IRestResponse response, RestRequestAsyncHandle handle, Action<int> callback) {
        if (!CheckRequestStatus(response, handle)) {
            return;
        }

        try {
            callback(Convert.ToInt32(response.Content));
        } catch (Exception e) {
            Tell.Err($"Error while parsing rating with error: [{e.Message}], trace: ___{e.StackTrace}___, and content request: ", response);
            throw;
        }
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

        try {
            callback();
        } catch (Exception e) {
            Tell.Err($"Error while calling rating set callback with error: [{e.Message}], trace: ___{e.StackTrace}___, and content request: ",
                response);
            throw;
        }
    }

    public static void SetRating(long storyId, int rating, Action setRatingCallback) {
        SteamAuth.GetEncodedTicket(ticket => { AfterEncodePut(storyId, rating, ticket); });

        void AfterEncodePut(long l, int i, string ticket) {
            Client.Put(
                $"storybook/story/{l}/rating/{i}",
                (response, handle) => { SetRatingCallback(response, handle, setRatingCallback); },
                ticket);
        }
    }

    private static bool CheckRequestStatus(IRestResponse response, RestRequestAsyncHandle handle = null) {            
        if (response.ResponseStatus != ResponseStatus.Completed) {
            Tell.Warn($"Rest call failed, {response.ResponseStatus}", response);
            if (handle != null) {
                Tell.Warn("^Continue", handle);
            }
            return false;
        }

        if (response.StatusCode != HttpStatusCode.OK) {
            Tell.Warn($"Rest call failed, {response.StatusCode}", response);
            if (handle != null) {
                Tell.Warn("^Continue", handle);
            }
            return false;
        }

        return true;
    }
}