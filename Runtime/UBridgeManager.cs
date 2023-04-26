using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System;
using WebSocketSharp.Server;
using System.Net.NetworkInformation;
using WebSocketSharp;

namespace MischievousByte.UBridge
{
    public static class UBridgeManager
    {
        public class Laputa : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                var msg = e.Data == "BALUS"
                          ? "Are you kidding?"
                          : "I'm not available now.";

                Send(msg);
            }

            protected override void OnOpen()
            {
                Debug.Log("OnOpen!");
                base.OnOpen();
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void OnLoad() { } //Trick to call the static constructor


        private static WebSocketServer server;
        
        static UBridgeManager()
        {
            if(!NetworkInterface.GetIsNetworkAvailable())
            {
                Debug.LogError("Failed to create websocket server: not connected to a network");
                return;
            }

            
            server = new WebSocketServer($"ws://{GetLocalIP()}:{FindAvailablePort()}");
            Debug.Log(server.Port);

            server.AddWebSocketService<Laputa>("/");
            server.Start();

            Debug.Log(server.IsListening);
            Application.quitting += OnQuit;
        }

        
        private static void OnQuit()
        {
            Application.quitting -= OnQuit;

            server.Stop();
            Debug.Log(server.IsListening);
        }


        private static string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private static int FindAvailablePort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }
    }
}
