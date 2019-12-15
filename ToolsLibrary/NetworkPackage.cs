using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ToolsLibrary
{
    [Serializable()]
    public class NetworkPackage : ISerializable
    {
        public AddressPart AddressPart { get; private set; }
        public MessageTypes MessageType { get; private set; }
        public string Message { get; private set; }
        public Stack<int> LabelStack { get; private set; }

        public enum MessageTypes
        {
            NodeHelloMessage = 0,
            MGMTHelloMessage,
            ClientToClientMessage,
            MGMTMessage
        }

        private NetworkPackage()
        {
            this.LabelStack = new Stack<int>();
        }

        public static NetworkPackage Clone(NetworkPackage networkPackage)
        {
            NetworkPackage result = new NetworkPackage
            {
                AddressPart = networkPackage.AddressPart,
                MessageType = networkPackage.MessageType,
                Message = networkPackage.Message,
                LabelStack = new Stack<int>()
            };

            int[] tempTable = networkPackage.LabelStack.ToArray();
            for (int i=0; i<tempTable.Length; i++)
            {
                result.LabelStack.Push(tempTable[i]);
            }
            return result;
        }

        public static NetworkPackage CreateClientToClientMessage(AddressPart addressPart, string message, int startLabel)
        {
            NetworkPackage result = new NetworkPackage
            {
                AddressPart = addressPart,
                Message = message,
                MessageType = MessageTypes.ClientToClientMessage
            };
            result.LabelStack.Push(startLabel);

            return result;
        }

        public static NetworkPackage CreateNodeHello(AddressPart addressPart)
        {
            NetworkPackage result = new NetworkPackage
            {
                AddressPart = addressPart,
                Message = "",
                MessageType = MessageTypes.NodeHelloMessage
            };
            return result;
        }

        public static NetworkPackage CreateMGMTHello(AddressPart addressPart)
        {
            NetworkPackage result = new NetworkPackage
            {
                AddressPart = addressPart,
                Message = "",
                MessageType = MessageTypes.MGMTHelloMessage
            };
            return result;
        }

        public static NetworkPackage CreateMGMTMessage(AddressPart addressPart, string message)
        {
            NetworkPackage result = new NetworkPackage
            {
                AddressPart = addressPart,
                Message = message,
                MessageType = MessageTypes.MGMTHelloMessage
            };
            return result;
        }

        public NetworkPackage(SerializationInfo serializationInfo, StreamingContext context)
        {
            AddressPart = (AddressPart)serializationInfo.GetValue("addressPart", typeof(AddressPart));
            Message = (string)serializationInfo.GetValue("message", typeof(string));
            MessageType = (MessageTypes)serializationInfo.GetValue("MessageType", typeof(int));
            LabelStack = (Stack<int>)serializationInfo.GetValue("labelStack", typeof(Stack<int>));
        }

        public void GetObjectData(SerializationInfo serializationInfo, StreamingContext context)
        {
            if (serializationInfo is null)
            {
                throw new ArgumentNullException(nameof(serializationInfo));
            }

            serializationInfo.AddValue("addressPart", AddressPart);
            serializationInfo.AddValue("message", Message);
            serializationInfo.AddValue("MessageType", MessageType);
            serializationInfo.AddValue("labelStack", LabelStack);
        }

    }

    [Serializable()]
    public class AddressPart : ISerializable
    {
        public string SenderId { get; private set; }
        public string CurrentIPAddress { get; set; }
        public int CurrentPort { get; set; } //needed considering project requirements; cloud have to know which port the package came out of the node
        public string ReceiverId { get; private set; }
        public string ReceiverIPAddress { get; private set; }

        public void GetObjectData(SerializationInfo serializationInfo, StreamingContext context)
        {
            if (serializationInfo is null)
            {
                throw new ArgumentNullException(nameof(serializationInfo));
            }

            serializationInfo.AddValue("senderId", SenderId);
            serializationInfo.AddValue("currentIPAddress", CurrentIPAddress);
            serializationInfo.AddValue("currentPort", CurrentPort);
            serializationInfo.AddValue("receiverId", ReceiverId);
            serializationInfo.AddValue("receiverIPAddress", ReceiverIPAddress);
        }

        public AddressPart(SerializationInfo serializationInfo, StreamingContext context)
        {
            SenderId = (string)serializationInfo.GetValue("senderId", typeof(string));
            CurrentIPAddress = (string)serializationInfo.GetValue("currentIPAddress", typeof(string));
            CurrentPort = (int)serializationInfo.GetValue("currentPort", typeof(int));
            ReceiverId = (string)serializationInfo.GetValue("receiverId", typeof(string));
            ReceiverIPAddress = (string)serializationInfo.GetValue("receiverIPAddress", typeof(string));

        }
        private AddressPart(string senderId, string receiverId, string currentIPAddress, int currentPort, string receiverIPAddress)
        {
            this.SenderId = senderId;
            this.CurrentIPAddress = currentIPAddress;
            this.ReceiverId = receiverId;
            this.ReceiverIPAddress = receiverIPAddress;
            this.CurrentPort = currentPort;
        }

        public static AddressPart CreateNetworkAddressPart(string senderId, string receiverId, string currentIPAddress, string receiverIPAddress, int currentPort) 
            => new AddressPart(senderId, receiverId, currentIPAddress, currentPort, receiverIPAddress);

        public static AddressPart CreateNetworkHelloAddressPart(string senderId, string currentIPAddress) 
            => new AddressPart(senderId, "Cloud", currentIPAddress, 0, "");

        public static AddressPart CreateMGMTHelloAddressPart(string senderId) 
            => new AddressPart(senderId, "Cloud", "0", 0, "");

        public static AddressPart CreateMGMTAddressPart(string senderId, string receiverId) 
            => new AddressPart(senderId, receiverId, "", 0, "");
    }
}
