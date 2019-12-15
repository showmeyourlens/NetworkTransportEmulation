using System;
using System.Collections.Generic;
using System.Text;

namespace CableCloud
{
    public class Link
    {
        public int LinkId { get; private set; }
        public string[] ConnectedNodes { get; private set; }
        public int[] ConnectedPorts { get; private set; }
        public Link(int linkId, string firstNode, string secondNode, int firstNodePort, int secondNodePort)
        {
            this.LinkId = linkId;
            this.ConnectedNodes = new string[2];
            this.ConnectedNodes[0] = firstNode;
            this.ConnectedNodes[1] = secondNode;
            this.ConnectedPorts = new int[2];
            this.ConnectedPorts[0] = firstNodePort;
            this.ConnectedPorts[1] = secondNodePort;
        }
        
    }
}
