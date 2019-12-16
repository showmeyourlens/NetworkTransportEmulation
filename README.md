#

This is an university project. I saw it once on my disc and decided to write it again, to make it cleaner and better.

Main purpose was to create MPLS network emulation. Such emulation uses many instances of routers and client nodes. They communicate with each other by sending, creating and receiving packets.

#How it works?

##Sending packet
All nodes have assigned emulation port numbers. Each connection in emulated network is represented by a port number. Let's assume that node A and B are connected directly and capable of creating packets. If node A wants to send packet to node B, it creates a packet with proper port number, called just "port". Port is just one, or chosen from FIB table, loaded when intialising node. FIB table tells which port to use under defined conditions. Then, packet with port is sent to CableCloud with node's only Socket (there is only one socket per node).

##"Physical" packet passing
CableCloud is the only element that has many Sockets (more precisely - as many as number of nodes in emulated network). CableCloud has  loaded routing table (as List of objects), which (objects) contains input ports, output ports, Sockets and more. This table tells "If the received packet has port number X, you should send it to Socket assigned to node B, with port Y". Y is one of node B ports. In this way CableCloud is physically routing packets in emulated network, using port numbers.

##Why?
This was a project requirement made by lecturer. It decreases number of Sockets used in project.

#Network Nodes description

##CableCloud
Actually, Cloud was covered in previous paragraph. 

##ClientNode
Represents device connected to network. This node is capable of creating packets. It uses ClientCloudCommunicator to send and receive them. Another one requirement was to send MPLS-labeled packets - apparently that was simpler (?)

##RouterNode
Has loaded FIB table that tells what action to perform when receiving packet with specific port and label. Action includes label switching, adding and deleting. Action (or actions chain) ends with passing packet to Cloud with port number changed.

