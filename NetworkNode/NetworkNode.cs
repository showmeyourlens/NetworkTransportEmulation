using NetworkNode;
using System;
using System.Collections.Generic;
using ToolsLibrary;

namespace NetworkNode
{
    class NetworkNode
    {           
        static void Main(string[] args)
        {
            NodeCloudCommunication cloudCommunicator = new NodeCloudCommunication(args[0], args[1], args[2]);

            Console.WriteLine("Starting network node with following parameters:");
            Console.WriteLine("Address on device: {0}:{1}", cloudCommunicator.instanceAddress, cloudCommunicator.instancePort);
            Console.WriteLine("Address in emulated network: {0}", cloudCommunicator.NODE_EMULATION_ADDRESS);
            Console.WriteLine("Node identificator: {0}", cloudCommunicator.NODE_EMULATION_ID);
            Console.WriteLine();
            Console.WriteLine("Press 'c' to close, anything else to turn router on/off");

            cloudCommunicator.Start();
            char key = Console.ReadKey().KeyChar;
            while(key != 'c')
            {
                cloudCommunicator.isRouterUp = !cloudCommunicator.isRouterUp;
                string message = cloudCommunicator.isRouterUp ? "Router turned on" : "Router turned off";
                Console.WriteLine(message);
                key = Console.ReadKey().KeyChar;
            }

            cloudCommunicator.Stop();
            Console.WriteLine("Closing");
            Console.ReadKey();
        }
    }
}
