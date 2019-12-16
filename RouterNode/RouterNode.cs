using RouterNode;
using System;
using System.Collections.Generic;
using ToolsLibrary;

namespace RouterNode
{
    class RouterNode
    {           
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Wrong parameters quantity. Shutting down.");
                return;
            }

            string instancePort = args[0];
            string nodeId = args[1];
            string nodeEmulationAddress = args[2];

            RouterCloudCommunication cloudCommunicator = new RouterCloudCommunication(instancePort, nodeId, nodeEmulationAddress);

            Console.WriteLine("Starting network node with following parameters:");
            Console.WriteLine("Address on device: {0}:{1}", "127.0.0.1", cloudCommunicator.instancePort);
            Console.WriteLine("Address in emulated network: {0}", nodeEmulationAddress);
            Console.WriteLine("Router identificator: {0}", nodeId);
            Console.WriteLine();
            Console.WriteLine("Press 'c' to close, anything else to turn router on/off");

            cloudCommunicator.Start();
            char key = Console.ReadKey().KeyChar;
            while(key != 'c')
            {
                cloudCommunicator.IsRouterUp = !cloudCommunicator.IsRouterUp;
                string message = cloudCommunicator.IsRouterUp ? "Router turned on" : "Router turned off";
                Console.WriteLine(message);
                key = Console.ReadKey().KeyChar;
            }

            cloudCommunicator.Stop();
            Console.WriteLine("Closing");
            Console.ReadKey();
        }
    }
}
