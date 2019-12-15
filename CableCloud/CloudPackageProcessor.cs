using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ToolsLibrary;

namespace CableCloud
{
    public class CloudPackageProcessor
    {
        private static string _THIS_COMPONENT_NAME = "Cloud";
        private List<TargetNetworkObject> _targetNetworkObjects;

        public CloudPackageProcessor()
        {
            CloudConnectionsXMLReader reader = new CloudConnectionsXMLReader();
            List<Link> networkLinks = reader.ReadCloudConnections();
            GenerateTargetObjectsList(networkLinks);
            reader.UpdateTargetsWithIPs(_targetNetworkObjects);
        }

        public ProcessorResponse ProcessPackageAndResponse(Socket socket, NetworkPackage receivedPackage)
        {
            ProcessorResponse response = receivedPackage.MessageType switch
            {
                NetworkPackage.MessageTypes.MGMTHelloMessage => ProcessHelloMessage(socket, receivedPackage),
                NetworkPackage.MessageTypes.NodeHelloMessage => ProcessHelloMessage(socket, receivedPackage),
                NetworkPackage.MessageTypes.ClientToClientMessage => ProcessClientToClientMessage(receivedPackage),
                NetworkPackage.MessageTypes.MGMTMessage => ProcessMGMTMessage(socket, receivedPackage),
                _ => ProcessorResponse.CreateDiscardingProcessorResponse(receivedPackage, "Unrecognised package type"),
            };
            return response;
        }

        private ProcessorResponse ProcessHelloMessage(Socket socket, NetworkPackage receivedPackage)
        {
            try
            {
                if (receivedPackage.MessageType.Equals(NetworkPackage.MessageTypes.MGMTHelloMessage))
                {
                    _targetNetworkObjects.Find(x => x.TargetObjectId == receivedPackage.AddressPart.SenderId).TargetSocket = socket;
                }
                else
                {
                    // All objects where sender is the target need to be updated.
                    var relatedTargetObjects = _targetNetworkObjects.FindAll(x => x.TargetObjectId.Equals(receivedPackage.AddressPart.SenderId));
                    if (!relatedTargetObjects.Any())
                    {
                        // if no object is found, it denotes a problem with emulation config.
                        throw new Exception();
                    }

                    relatedTargetObjects.All(x => { x.TargetSocket = socket; return true; } );
                    TimeStamp.WriteLine("{0} using address {1} connected to cloud.", receivedPackage.AddressPart.SenderId, receivedPackage.AddressPart.CurrentIPAddress);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Received \"Hello\" message from unrecognised object");
                AddressPart addressPart = AddressPart.CreateMGMTAddressPart(_THIS_COMPONENT_NAME, receivedPackage.AddressPart.SenderId);
                NetworkPackage networkPackage = NetworkPackage.CreateMGMTMessage(addressPart, "Object not recognised by cloud");

                return ProcessorResponse.CreateProcessorResponse(socket, networkPackage);
            }

            return ProcessorResponse.CreateProcessorResponse(socket, receivedPackage);
        }

        private ProcessorResponse ProcessMGMTMessage(Socket socket, NetworkPackage receivedPackage)
        {
            if (receivedPackage.Message == "ROUTERS")
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
                    return ProcessorResponse.CreateProcessorResponse(targets.Select(x => x.TargetSocket).ToList(), receivedPackage);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return ProcessorResponse.CreateDiscardingProcessorResponse(receivedPackage, "Broadcast MGMT failed");
                }
            }
            else
            {
                return ProcessorResponse.CreateDiscardingProcessorResponse(receivedPackage, "Unrecognised receiver / receivers group");
            }
        }

        private ProcessorResponse ProcessClientToClientMessage(NetworkPackage receivedPackage)
        {
            TargetNetworkObject nextNetworkNode = _targetNetworkObjects.Find(x => x.InputPort == receivedPackage.AddressPart.CurrentPort);
            if (nextNetworkNode != null)
            {
                TimeStamp.WriteLine("Received package from {0}", String.Concat(receivedPackage.AddressPart.CurrentIPAddress, ":", receivedPackage.AddressPart.CurrentPort));
                if (nextNetworkNode.TargetSocket != null)
                {
                    receivedPackage.AddressPart.CurrentIPAddress = nextNetworkNode.TargetObjectAddress;
                    receivedPackage.AddressPart.CurrentPort = nextNetworkNode.TargetPort;
                    Console.WriteLine("{0} Passing packet to {1}", TimeStamp.TAB, String.Concat(nextNetworkNode.TargetObjectAddress, ":", nextNetworkNode.TargetPort));
                    return ProcessorResponse.CreateProcessorResponse(nextNetworkNode.TargetSocket, receivedPackage);
                }
                else
                {
                    return ProcessorResponse.CreateDiscardingProcessorResponse(receivedPackage, String.Format("Socket for target node {0} is null", nextNetworkNode.TargetObjectId));
                }
            }
            else
            {
                return ProcessorResponse.CreateDiscardingProcessorResponse(receivedPackage, String.Format("Target for port {0} not found. XML problem?", receivedPackage.AddressPart.CurrentPort));
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
