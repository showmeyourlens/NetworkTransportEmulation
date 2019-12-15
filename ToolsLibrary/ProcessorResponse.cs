using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ToolsLibrary
{
    public class ProcessorResponse
    {
        public readonly string discardMessage;
        public readonly List<Socket> socketToSend;
        public readonly NetworkPacket networkPacket;

        private ProcessorResponse(Socket socketToSend, NetworkPacket networkPacket, string errorMessage)
        {
            this.socketToSend = new List<Socket> { socketToSend };
            this.networkPacket = networkPacket;
            this.discardMessage = errorMessage;
        }

        private ProcessorResponse(List<Socket> socketToSend, NetworkPacket networkPacket, string errorMessage)
        {
            this.socketToSend = socketToSend;
            this.networkPacket = networkPacket;
            this.discardMessage = errorMessage;
        }

        private ProcessorResponse(NetworkPacket networkPacket, string errorMessage)
        {
            this.networkPacket = networkPacket;
            this.discardMessage = errorMessage;
        }

        public static ProcessorResponse CreateProcessorResponse(Socket socketToSend, NetworkPacket networkPacket)
            => new ProcessorResponse(socketToSend, networkPacket, null);

        public static ProcessorResponse CreateProcessorResponse(List<Socket> socketToSend, NetworkPacket networkPacket) 
            => new ProcessorResponse(socketToSend, networkPacket, null);
        public static ProcessorResponse CreateDiscardingProcessorResponse(NetworkPacket networkPacket, string discardMessage) 
            => new ProcessorResponse(networkPacket, discardMessage);
    }
}
