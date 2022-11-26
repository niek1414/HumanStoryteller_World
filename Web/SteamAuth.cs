using System;
using System.Collections.Generic;
using Steamworks;
using Verse.Steam;

namespace HumanStoryteller.Web; 
public abstract class SteamAuth {
    protected static Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponse;
    private static byte[] _mTicket;
    private static uint _mPcbTicket;
    private static HAuthTicket _mHAuthTicket;
    private static List<Action<string>> callbackQueue = new List<Action<string>>();
    private static DateTime lastUpdate = DateTime.Now;

    /// <summary>
    /// MUST call callback with encoded ticket
    /// </summary>
    /// <param name="returnCallback"></param>
    public static void GetEncodedTicket(Action<string> returnCallback) {
        if (_mTicket != null && lastUpdate.AddMinutes(3) > DateTime.Now) {
            OnGetAuthSessionTicketResponse(new GetAuthSessionTicketResponse_t(), returnCallback);
            return;
        }

        if (SteamManager.Initialized) {
            m_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(c => {
                OnGetAuthSessionTicketResponse(c, returnCallback);
            });

            _mTicket = new byte[1024];
            _mHAuthTicket = SteamUser.GetAuthSessionTicket(_mTicket, 1024, out _mPcbTicket);
        }
    }

    private static void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback, Action<string> returnCallback) {
        callbackQueue.Add(returnCallback);

        if (_mTicket.Length <= 0) {
            return;
        }

        Array.Resize(ref _mTicket, (int) _mPcbTicket);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (byte b in _mTicket) {
            sb.AppendFormat("{0:x2}", b);
        }

        callbackQueue.ForEach(callback => callback(sb.ToString()));
        callbackQueue.Clear();
    }
}