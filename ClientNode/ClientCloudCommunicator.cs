using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ToolsLibrary;

namespace ClientNode
{
    public class ClientCloudCommunicator : CloudCommunicator
    {
        private readonly string NODE_EMULATION_ID;
        private readonly string NODE_EMULATION_ADDRESS;
        public readonly int NODE_EMULATION_PORT;
        public ClientCloudCommunicator(string instancePort, string nodeId, string nodeEmulationAddress, string nodeEmulationPort) : base(instancePort)
        {
            NODE_EMULATION_ID = nodeId;
            NODE_EMULATION_ADDRESS = nodeEmulationAddress;
            NODE_EMULATION_PORT = Int32.Parse(nodeEmulationPort);
        }

        public override NetworkPacket CreateHelloMessage()
        {
            AddressPart addressPart = AddressPart.CreateNetworkHelloAddressPart(NODE_EMULATION_ID, NODE_EMULATION_ADDRESS);
            return NetworkPacket.CreateNodeHello(addressPart);
        }

        public override void ProcessReceivedClientMessage(NetworkPacket networkPacket) 
            => TimeStamp.WriteLine("Received message from {0}. Message: {1}", networkPacket.AddressPart.SenderId, networkPacket.Message);

        public override void ProcessReceivedManagementMessage(NetworkPacket networkPacket) 
            => TimeStamp.WriteLine(networkPacket.Message);
    }
}
