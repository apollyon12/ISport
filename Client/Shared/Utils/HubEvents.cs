using System;

//using Microsoft.AspNetCore.SignalR.Client;

using iSportsRecruiting.Shared.Models;

namespace iSportsRecruiting.Client.Shared.Utils
{
    public static class HubEvents
    {
        public static bool IsInChat;

        public static event Action<int> Ping;
        public static event Action<int> Pong;
        public static event Action<int> IsWriting;
        public static event Action<EmailModel> ReceivedEmail;

        // private static HubConnection Connection;
        // public static void SetConnection(HubConnection connection)
        // {
        //     Connection = connection;
        //
        //     Connection.On<EmailModel>("EmailReceived", email => 
        //         ReceivedEmail?.Invoke(email));
        //
        //     Connection.On<int>("Ping", id =>
        //         Ping?.Invoke(id));
        //
        //     Connection.On<int>("Pong", id =>
        //         Pong?.Invoke(id));
        // }

        public static void SetWritingNotifications(string id)
        {
            // Connection.On<int>(id, _id =>
            //     IsWriting?.Invoke(_id));
        }

        public static void CloseChat()
        {
            if (ReceivedEmail != null)
                foreach (var d in ReceivedEmail.GetInvocationList())
                    ReceivedEmail -= d as Action<EmailModel>;

            if (Ping != null)
                foreach (var d in Ping.GetInvocationList())
                    Ping -= d as Action<int>;

            if (Pong != null)
                foreach (var d in Pong.GetInvocationList())
                    Pong -= d as Action<int>;

            if (IsWriting != null)
                foreach (var d in IsWriting.GetInvocationList())
                    IsWriting -= d as Action<int>;
        }
    }
}
