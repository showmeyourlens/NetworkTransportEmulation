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
        public readonly NetworkPackage networkPackage;

        private ProcessorResponse(Socket socketToSend, NetworkPackage networkPackage, string errorMessage)
        {
            this.socketToSend = new List<Socket> { socketToSend };
            this.networkPackage = networkPackage;
            this.discardMessage = errorMessage;
        }

        private ProcessorResponse(List<Socket> socketToSend, NetworkPackage networkPackage, string errorMessage)
        {
            this.socketToSend = socketToSend;
            this.networkPackage = networkPackage;
            this.discardMessage = errorMessage;
        }

        private ProcessorResponse(NetworkPackage networkPackage, string errorMessage)
        {
            this.networkPackage = networkPackage;
            this.discardMessage = errorMessage;
        }

        public static ProcessorResponse CreateProcessorResponse(Socket socketToSend, NetworkPackage networkPackage)
            => new ProcessorResponse(socketToSend, networkPackage, null);

        public static ProcessorResponse CreateProcessorResponse(List<Socket> socketToSend, NetworkPackage networkPackage) 
            => new ProcessorResponse(socketToSend, networkPackage, null);
        public static ProcessorResponse CreateDiscardingProcessorResponse(NetworkPackage networkPackage, string discardMessage) 
            => new ProcessorResponse(networkPackage, discardMessage);
    }
}
