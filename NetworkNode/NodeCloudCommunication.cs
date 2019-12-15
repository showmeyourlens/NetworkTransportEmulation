using NetworkNode;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;

namespace NetworkNode
{
    public class NodeCloudCommunication : CloudCommunicator
    {
        public FIBXMLReader Reader;
        public RoutingInfo routingInfo;
        public bool isRouterUp;    

        public NodeCloudCommunication(string instancePort, string nodeId, string nodeEmulationAddress) : base(instancePort, nodeId, nodeEmulationAddress)
        {
            this.instanceAddress = IPAddress.Parse("127.0.0.1");
            this.cloudPort = 62572;
            this.instancePort = Int32.Parse(instancePort);
            this.NODE_EMULATION_ID = nodeId;
            this.NODE_EMULATION_ADDRESS = nodeEmulationAddress;
            this.Reader = new FIBXMLReader();
            this.routingInfo = Reader.ReadFIB("ManagementSystem.xml", nodeId);
            this.sendDone = new ManualResetEvent(false);
            this.isRouterUp = true;
        }

        public override NetworkPackage CreateHelloMessage()
        {
            AddressPart addressPart = AddressPart.CreateNetworkHelloAddressPart(NODE_EMULATION_ID, NODE_EMULATION_ADDRESS);
            return NetworkPackage.CreateNodeHello(addressPart);
        }
        public override void ProcessReceivedClientMessage(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("Received packet to {0} on port {1}", networkPackage.AddressPart.ReceiverIPAddress, networkPackage.AddressPart.CurrentPort);
            // RISKY - if message from client, add label "0" (e.g. assume there is one)
            if (networkPackage.LabelStack.Count == 0)
            {
                networkPackage.LabelStack.Push(0);
            }

            int topLabel = networkPackage.LabelStack.Peek();
            
            RouterLabel routerLabel = routingInfo.routerLabels.Find(x => x.inputPort == networkPackage.AddressPart.CurrentPort && x.label == topLabel);
            RouterAction action = routerLabel != null ? routingInfo.routerActions.Find(x => x.actionId == routerLabel.action) : null;
            
            
            if (DoSelectedAction(action, networkPackage))
            {
                Console.WriteLine("{0} Passing packet on port {1}.", TimeStamp.TAB, networkPackage.AddressPart.CurrentPort);
                Send(networkPackage);
            }
            
        }
        public override void ProcessReceivedManagementMessage(NetworkPackage networkPackage)
        {
            if (networkPackage.Message.Equals("ROUTING_SCHEME_2"))
            {
                TimeStamp.WriteLine("MANAGEMENT MESSAGE: upload new forwarding table");
                routingInfo = Reader.ReadFIB("ManagementSystem2.xml", NODE_EMULATION_ID);
                Console.WriteLine("{0} upload done. ", TimeStamp.TAB);
            }
        }

        private bool DoSelectedAction(RouterAction action, NetworkPackage networkPackage)
        {
            if (action == null)
            {
                TimeStamp.WriteLine("ERROR - no action defined (port {0}, label {1}). Discarded");
                return false;
            }
            try
            {
                switch (action.actionString)
                {
                    case "POP":
                        int deletedLabel = networkPackage.LabelStack.Pop();
                        Console.WriteLine("{0} Deleted label {1}, considering label {2}", TimeStamp.TAB, deletedLabel, networkPackage.LabelStack.Peek());
                        action = routingInfo.routerActions.Find(x => x.actionId == routingInfo.routerLabels.Find(y => y.label == networkPackage.LabelStack.Peek()).action);
                        DoSelectedAction(action, networkPackage);
                        break;
                    case "SWAP":
                        Console.WriteLine("{0} Label {1} switched to {2}", TimeStamp.TAB, networkPackage.LabelStack.Pop(), action.outLabel);
                        networkPackage.LabelStack.Push(action.outLabel);
                        break;
                    case "PUSH":
                        Console.WriteLine("{0} Added label {1}", TimeStamp.TAB, action.outLabel);
                        networkPackage.LabelStack.Push(action.outLabel);
                        break;
                }
            }
            catch (Exception e)
            {
                TimeStamp.WriteLine("Error with label stack");
                Console.WriteLine(e.Message);
                return false;

            }
            if(action.nextActionId != 0)
            {
                DoSelectedAction(routingInfo.routerActions.Find(x => x.actionId == action.nextActionId), networkPackage);
            }
            if(action.outPort != 0)
            {
                networkPackage.AddressPart.CurrentPort = action.outPort;
                networkPackage.AddressPart.CurrentIPAddress = NODE_EMULATION_ADDRESS;
            }
            return true;

        }
    }
}

