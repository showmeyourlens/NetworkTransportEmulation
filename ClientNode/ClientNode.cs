using ClientNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ToolsLibrary;

namespace TSST_MPLS
{
    class ClientNode
    {
        public List<ClientSenderConfig> contactList;
        static void Main(string[] args)
        {
            ClientNode clientNode = new ClientNode();
            if (args.Length != 4)
            {
                Console.WriteLine("Wrong parameters quantity. Shutting down.");
                return;
            }

            clientNode.contactList = clientNode.CreateDumbClientConfig(args[1]);

            ClientCloudCommunicator cloudCommunicator = new ClientCloudCommunicator(args[0], args[1], args[2], args[3]);
            
            Console.WriteLine("Starting client node with following parameters:");
            Console.WriteLine("Address on device: {0}:{1}", cloudCommunicator.instanceAddress, cloudCommunicator.instancePort);
            Console.WriteLine("Address in emulated network: {0}:{1}", cloudCommunicator.NODE_EMULATION_ADDRESS, cloudCommunicator.NODE_EMULATION_PORT);
            Console.WriteLine("Node identificator: {0}", cloudCommunicator.NODE_EMULATION_ID);
            
            try
            {
                Console.WriteLine("Client is up!");
                cloudCommunicator.Start();
                Console.WriteLine("Type number of client for sending message, c to close");
                bool isFinish = true;
                char key;
                while (isFinish)
                {
                    key = Console.ReadKey().KeyChar;
                    ClientSenderConfig contact = clientNode.contactList.FirstOrDefault(x => x.key == key);
                    if (contact != null)
                    {
                        AddressPart addressPart = AddressPart.CreateNetworkAddressPart(
                            cloudCommunicator.NODE_EMULATION_ID,
                            contact.receiverId,
                            cloudCommunicator.NODE_EMULATION_ADDRESS,
                            contact.receiverIPAddress,
                            cloudCommunicator.NODE_EMULATION_PORT);
                        NetworkPacket networkPacket = NetworkPacket.CreateClientToClientMessage(
                            addressPart,
                            "Very Important message",
                            contact.label);
                        cloudCommunicator.Send(networkPacket);
                        Console.WriteLine("Message sent");
                    }
                    else
                    {
                        Console.WriteLine("Client with this number hasn't been found in contact book");
                    }

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Cloud communication is down.");
            }

            cloudCommunicator.Stop();
            Console.WriteLine("Closing");
            Console.ReadKey();
        }

        private List<ClientSenderConfig> CreateDumbClientConfig(string thisNodeId)
        {
            List<ClientSenderConfig> result = new List<ClientSenderConfig>();
            switch(thisNodeId)
            {
                case "C1":
                    result.Add(new ClientSenderConfig("C2", "171.18.151.27", 17));
                    result.Add(new ClientSenderConfig("C3", "148.58.132.21", 18));
                    result.Add(new ClientSenderConfig("C4", "132.41.56.205", 19));
                    break;
                case "C2":
                    result.Add(new ClientSenderConfig("C1", "121.32.232.31", 21));
                    result.Add(new ClientSenderConfig("C3", "148.58.132.21", 23));
                    result.Add(new ClientSenderConfig("C4", "132.41.56.205", 24));
                    break;
                case "C3":
                    result.Add(new ClientSenderConfig("C1", "121.32.232.31", 38));
                    result.Add(new ClientSenderConfig("C2", "171.18.151.27", 39));
                    result.Add(new ClientSenderConfig("C4", "132.41.56.205", 40));
                    break;
                case "C4":
                    result.Add(new ClientSenderConfig("C1", "121.32.232.31", 41));
                    result.Add(new ClientSenderConfig("C2", "171.18.151.27", 42));
                    result.Add(new ClientSenderConfig("C3", "148.58.132.21", 43));
                    break;
            }
            return result;
        }
    }
    class ClientSenderConfig
    {
        public readonly char key;
        public readonly string receiverId;
        public readonly string receiverIPAddress;
        public readonly int label;

        public ClientSenderConfig(string receiverId, string receiverIPAddress, int label)
        {
            this.key = (char)receiverId[1];
            this.receiverId = receiverId;
            this.receiverIPAddress = receiverIPAddress;
            this.label = label;
        }

    }
}
