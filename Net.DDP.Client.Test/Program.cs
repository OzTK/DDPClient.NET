﻿using System;
using System.Collections.Generic;

namespace Net.DDP.Client.Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            // Testing by listing all atmosphere packages
            DDPClient client = new DDPClient(new Subscriber());
            client.Connect("atmosphere.meteor.com:443");
            client.Subscribe("packages");
            Console.ReadLine();
        }
    }

    public class Subscriber : IDataSubscriber
    {
        // Atmosphere packages list
        private List<String> _packages = new List<String>();
        private string _sessionId;

        public void DataReceived(dynamic data)
        {
            try
            {
                // Handling connection to server
                if (data.type == DDPClient.DDP_TYPE_CONNECTED)
                {
                    _sessionId = data.session;
                    Console.WriteLine("Connected! Session id: " + _sessionId);
                }
                else if (data.type == DDPClient.DDP_TYPE_ADDED) // Handling added event
                {
                    _packages.Add(data.name);
                    Console.Write(data.name + ", ");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error trying to parse data");
            }
        }

        public string Session { get; set; }
    }
}
