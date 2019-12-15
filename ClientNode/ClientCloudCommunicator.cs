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
        public int NODE_EMULATION_PORT;
        public ClientCloudCommunicator(string instancePort, string nodeId, string nodeEmulationAddress, string nodeEmulationPort) : base(instancePort, nodeId, nodeEmulationAddress)
        {
            base.instanceAddress = IPAddress.Parse("127.0.0.1");
            base.cloudPort = 62572;
            base.instancePort = Int32.Parse(instancePort);
            base.NODE_EMULATION_ID = nodeId;
            base.NODE_EMULATION_ADDRESS = nodeEmulationAddress;
            NODE_EMULATION_PORT = Int32.Parse(nodeEmulationPort);
        }

        public override NetworkPackage CreateHelloMessage()
        {
            AddressPart addressPart = AddressPart.CreateNetworkHelloAddressPart(NODE_EMULATION_ID, NODE_EMULATION_ADDRESS);
            return NetworkPackage.CreateNodeHello(addressPart);
        }

        public override void ProcessReceivedClientMessage(NetworkPackage networkPackage) 
            => TimeStamp.WriteLine("Received message from {0}. Message: {1}", networkPackage.AddressPart.SenderId, networkPackage.Message);

        public override void ProcessReceivedManagementMessage(NetworkPackage networkPackage) 
            => TimeStamp.WriteLine(networkPackage.Message);
    }
}
