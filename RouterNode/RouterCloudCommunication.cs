using RouterNode;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;

namespace RouterNode
{
    public class RouterCloudCommunication : CloudCommunicator
    {
        private readonly string NODE_EMULATION_ID;
        private readonly string NODE_EMULATION_ADDRESS;
        private readonly FIBXMLReader _reader;
        private RoutingInfo _routingInfo;
        public bool IsRouterUp { get; set; }    

        public RouterCloudCommunication(string instancePort, string nodeId, string nodeEmulationAddress) : base(instancePort)
        {
            this.NODE_EMULATION_ID = nodeId;
            this.NODE_EMULATION_ADDRESS = nodeEmulationAddress;
            this._reader = new FIBXMLReader();
            this._routingInfo = _reader.ReadFIB("ManagementSystem.xml", nodeId);
            this.sendDone = new ManualResetEvent(false);
            this.IsRouterUp = true;
        }

        public override NetworkPacket CreateHelloMessage()
        {
            AddressPart addressPart = AddressPart.CreateNetworkHelloAddressPart(NODE_EMULATION_ID, NODE_EMULATION_ADDRESS);
            return NetworkPacket.CreateNodeHello(addressPart);
        }
        public override void ProcessReceivedClientMessage(NetworkPacket networkPacket)
        {
            TimeStamp.WriteLine("Received packet to {0} on port {1}", networkPacket.AddressPart.ReceiverIPAddress, networkPacket.AddressPart.CurrentPort);
            // RISKY - if message from client, add label "0" (e.g. assume there is one)
            if (networkPacket.LabelStack.Count == 0)
            {
                networkPacket.LabelStack.Push(0);
            }

            int topLabel = networkPacket.LabelStack.Peek();
            
            RouterLabel routerLabel = _routingInfo.RouterLabels.Find(x => x.inputPort == networkPacket.AddressPart.CurrentPort && x.label == topLabel);
            RouterAction action = routerLabel != null ? _routingInfo.RouterActions.Find(x => x.actionId == routerLabel.action) : null;
            
            
            if (DoSelectedAction(action, networkPacket))
            {
                Console.WriteLine("{0} Passing packet on port {1}.", TimeStamp.TAB, networkPacket.AddressPart.CurrentPort);
                Send(networkPacket);
            }
            
        }
        public override void ProcessReceivedManagementMessage(NetworkPacket networkPacket)
        {
            if (networkPacket.Message.Equals("ROUTING_SCHEME_2"))
            {
                TimeStamp.WriteLine("MANAGEMENT MESSAGE: upload new forwarding table");
                _routingInfo = _reader.ReadFIB("ManagementSystem2.xml", NODE_EMULATION_ID);
                Console.WriteLine("{0} upload done. ", TimeStamp.TAB);
            }
        }

        private bool DoSelectedAction(RouterAction action, NetworkPacket networkPacket)
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
                        int deletedLabel = networkPacket.LabelStack.Pop();
                        Console.WriteLine("{0} Deleted label {1}, considering label {2}", TimeStamp.TAB, deletedLabel, networkPacket.LabelStack.Peek());
                        action = _routingInfo.RouterActions.Find(x => x.actionId == _routingInfo.RouterLabels.Find(y => y.label == networkPacket.LabelStack.Peek()).action);
                        DoSelectedAction(action, networkPacket);
                        break;
                    case "SWAP":
                        Console.WriteLine("{0} Label {1} switched to {2}", TimeStamp.TAB, networkPacket.LabelStack.Pop(), action.outLabel);
                        networkPacket.LabelStack.Push(action.outLabel);
                        break;
                    case "PUSH":
                        Console.WriteLine("{0} Added label {1}", TimeStamp.TAB, action.outLabel);
                        networkPacket.LabelStack.Push(action.outLabel);
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
                DoSelectedAction(_routingInfo.RouterActions.Find(x => x.actionId == action.nextActionId), networkPacket);
            }
            if(action.outPort != 0)
            {
                networkPacket.AddressPart.CurrentPort = action.outPort;
                networkPacket.AddressPart.CurrentIPAddress = NODE_EMULATION_ADDRESS;
            }
            return true;

        }
    }
}

