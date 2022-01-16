# MPLS network emulation 

This is an university project. I saw it once on my disc and decided to write it again, to make it cleaner and better.

Main purpose was to create MPLS network emulation. Such emulation uses many instances of routers and client nodes. They communicate with each other by sending, creating and receiving packets.

# How it works?

## "Sending" packet in emulated network
All nodes have assigned some emulation port numbers, one per each connected node (these are not actual ports) - see Topology.jpg. Ports are used by CableCloud to perform actual packet sending.

Let's assume that node A and B are connected directly (in emulation topology sense) and capable of creating packets. If node A wants to send packet to node B, it creates a packet with proper port number, called just "port". Port determines which wire (cable) in emulation would be used to send packet. RouterNodes have multiple ports and more sophisticated rules for sending packets, defined in FIB table. FIB table tells which port to use under defined conditions. Then, packet is sent to CableCloud.

Every node, except for CableCloud, has only one socket (project requirement - supposed to make it easier). This socket is using localhost address and actual port (in opposition to mentioned before emulation port). This socket is used to send and receive packets.

## "Physical" packet passing
CableCloud is the only element that has many Sockets (more precisely - as many as number of nodes in emulated network). Its purpose is to emulate connections (wires) between network nodes. CableCloud contains RoutingTable, loaded from XML file. This table tells "If the received packet has port number x, it should be send to Socket assigned to node B, and have port number changed to y". y is one of node B ports. In this way CableCloud is physically transmitting packets in emulated network, using emulation port numbers and sockets.

# Network Nodes description

## ClientNode
Represents client device connected to network. This node is capable of creating packets. It uses ClientCloudCommunicator to send and receive them. 

This node sends MPLS-labeled packets, despite it shouldn't - it's another project requirement.

## RouterNode
Represents Internet Provider router. Has loaded FIB table that tells what action to perform when receiving packet with specific port and label. Typical action would be: "If packet arrives on port 21 with most recent label 2, change most recent label to 5, push label 5 to label stack and send using port 6". Action includes label switching, adding and deleting.

# I want to try it!
Run bat.bat. Requires .NET Core 3.0 (sorry! You can change it to 3.1 in project files however). Multiple console windows should appear (consoles were another requirement). Then find any ClientNode window and try to send a message to other client. You can see what appears in other windows. Routers, cloud and of course target client are logging all actions in own console windows. After sending message, try to follow message route through nodes and compare it with topology in Topology.jpg
