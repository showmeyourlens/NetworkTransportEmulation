using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ToolsLibrary;

namespace CableCloud
{
    public class CloudPacketProcessor
    {
        private static string _THIS_COMPONENT_NAME = "Cloud";
        private List<TargetNetworkObject> _targetNetworkObjects;

        public CloudPacketProcessor()
        {
            CloudConnectionsXMLReader reader = new CloudConnectionsXMLReader();
            List<Link> networkLinks = reader.ReadCloudConnections();
            GenerateTargetObjectsList(networkLinks);
            reader.UpdateTargetsWithIPs(_targetNetworkObjects);
        }

        public ProcessorResponse ProcessPacketAndResponse(Socket socket, NetworkPacket receivedPacket)
        {
            ProcessorResponse response = receivedPacket.MessageType switch
            {
                NetworkPacket.MessageTypes.MGMTHelloMessage => ProcessHelloMessage(socket, receivedPacket),
                NetworkPacket.MessageTypes.NodeHelloMessage => ProcessHelloMessage(socket, receivedPacket),
                NetworkPacket.MessageTypes.ClientToClientMessage => ProcessClientToClientMessage(receivedPacket),
                NetworkPacket.MessageTypes.MGMTMessage => ProcessMGMTMessage(socket, receivedPacket),
                _ => ProcessorResponse.CreateDiscardingProcessorResponse(receivedPacket, "Unrecognised package type"),
            };
            return response;
        }

        private ProcessorResponse ProcessHelloMessage(Socket socket, NetworkPacket receivedPacket)
        {
            try
            {
                if (receivedPacket.MessageType.Equals(NetworkPacket.MessageTypes.MGMTHelloMessage))
                {
                    _targetNetworkObjects.Find(x => x.TargetObjectId == receivedPacket.AddressPart.SenderId).TargetSocket = socket;
                }
                else
                {
                    // All objects where sender is the target need to be updated.
                    var relatedTargetObjects = _targetNetworkObjects.FindAll(x => x.TargetObjectId.Equals(receivedPacket.AddressPart.SenderId));
                    if (!relatedTargetObjects.Any())
                    {
                        // if no object is found, it denotes a problem with emulation config.
                        throw new Exception();
                    }

                    relatedTargetObjects.All(x => { x.TargetSocket = socket; return true; } );
                    TimeStamp.WriteLine("{0} using address {1} connected to cloud.", receivedPacket.AddressPart.SenderId, receivedPacket.AddressPart.CurrentIPAddress);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Received \"Hello\" message from unrecognised object");
                AddressPart addressPart = AddressPart.CreateMGMTAddressPart(_THIS_COMPONENT_NAME, receivedPacket.AddressPart.SenderId);
                NetworkPacket networkPacket = NetworkPacket.CreateMGMTMessage(addressPart, "Object not recognised by cloud");

                return ProcessorResponse.CreateProcessorResponse(socket, networkPacket);
            }

            return ProcessorResponse.CreateProcessorResponse(socket, receivedPacket);
        }

        private ProcessorResponse ProcessMGMTMessage(Socket socket, NetworkPacket receivedPacket)
        {
            if (receivedPacket.Message == "ROUTERS")
            {
                try
                {
                    // Returns list of targets for all routers, one per router (ignoring ports). 
                    List<TargetNetworkObject> targets = _targetNetworkObjects
                        .FindAll(x => x.TargetObjectId[0] == 'N')
                        .GroupBy(x => x.TargetObjectId)
                        .Select(group => group.First())
                        .ToList();

                    //extracting list of sockets, one per router.
                    return ProcessorResponse.CreateProcessorResponse(targets.Select(x => x.TargetSocket).ToList(), receivedPacket);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return ProcessorResponse.CreateDiscardingProcessorResponse(receivedPacket, "Broadcast MGMT failed");
                }
            }
            else
            {
                return ProcessorResponse.CreateDiscardingProcessorResponse(receivedPacket, "Unrecognised receiver / receivers group");
            }
        }

        private ProcessorResponse ProcessClientToClientMessage(NetworkPacket receivedPacket)
        {
            TargetNetworkObject nextRouterNode = _targetNetworkObjects.Find(x => x.InputPort == receivedPacket.AddressPart.CurrentPort);
            if (nextRouterNode != null)
            {
                TimeStamp.WriteLine("Received package from {0}", String.Concat(receivedPacket.AddressPart.CurrentIPAddress, ":", receivedPacket.AddressPart.CurrentPort));
                if (nextRouterNode.TargetSocket != null)
                {
                    receivedPacket.AddressPart.CurrentIPAddress = nextRouterNode.TargetObjectAddress;
                    receivedPacket.AddressPart.CurrentPort = nextRouterNode.TargetPort;
                    Console.WriteLine("{0} Passing packet to {1}", TimeStamp.TAB, String.Concat(nextRouterNode.TargetObjectAddress, ":", nextRouterNode.TargetPort));
                    return ProcessorResponse.CreateProcessorResponse(nextRouterNode.TargetSocket, receivedPacket);
                }
                else
                {
                    return ProcessorResponse.CreateDiscardingProcessorResponse(receivedPacket, String.Format("Socket for target node {0} is null", nextRouterNode.TargetObjectId));
                }
            }
            else
            {
                return ProcessorResponse.CreateDiscardingProcessorResponse(receivedPacket, String.Format("Target for port {0} not found. XML problem?", receivedPacket.AddressPart.CurrentPort));
            }
        }

        private void GenerateTargetObjectsList(List<Link> networkLinks)
        {
            _targetNetworkObjects = new List<TargetNetworkObject>();
            // Creating target objects list. Note that all connections are added twice, to ease finding proper one.
            foreach (Link link in networkLinks)
            {
                _targetNetworkObjects.Add(new TargetNetworkObject(link.ConnectedPorts[0], link.ConnectedPorts[1], link.ConnectedNodes[1]));
                _targetNetworkObjects.Add(new TargetNetworkObject(link.ConnectedPorts[1], link.ConnectedPorts[0], link.ConnectedNodes[0]));
            }
            // Management node use special communication mode (directly to desired node)
            _targetNetworkObjects.Add(new TargetNetworkObject("MGMT"));
        }

        public void ShutDownSockets()
        {
            _targetNetworkObjects
                .FindAll(x => x.TargetSocket != null)
                .All(x => { x.TargetSocket.Disconnect(false); return true; });
        }
        public void DeleteDisconnectedSocket(Socket socket)
        {
            List<TargetNetworkObject> targetsToClearSocket = _targetNetworkObjects.FindAll(x => x.TargetSocket == socket);
            if(targetsToClearSocket.Any(x => x.TargetSocket != null) && socket != null)
            {
                targetsToClearSocket.ForEach(x => { x.TargetSocket = null; });
                TimeStamp.WriteLine("Node {0} has disconnected from cloud. Socket cleared", targetsToClearSocket.First().TargetObjectId);
            }
            else
            {
                TimeStamp.WriteLine("An unrecognised socket lost connection");
            }
        }
    }
}
