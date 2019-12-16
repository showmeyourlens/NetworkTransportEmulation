using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ToolsLibrary
{
    [Serializable()]
    public class NetworkPacket : ISerializable
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

        private NetworkPacket()
        {
            this.LabelStack = new Stack<int>();
        }

        public static NetworkPacket Clone(NetworkPacket networkPacket)
        {
            NetworkPacket result = new NetworkPacket
            {
                AddressPart = networkPacket.AddressPart,
                MessageType = networkPacket.MessageType,
                Message = networkPacket.Message,
                LabelStack = new Stack<int>()
            };

            int[] tempTable = networkPacket.LabelStack.ToArray();
            for (int i=0; i<tempTable.Length; i++)
            {
                result.LabelStack.Push(tempTable[i]);
            }
            return result;
        }

        public static NetworkPacket CreateClientToClientMessage(AddressPart addressPart, string message, int startLabel)
        {
            NetworkPacket result = new NetworkPacket
            {
                AddressPart = addressPart,
                Message = message,
                MessageType = MessageTypes.ClientToClientMessage
            };
            result.LabelStack.Push(startLabel);

            return result;
        }

        public static NetworkPacket CreateNodeHello(AddressPart addressPart)
        {
            NetworkPacket result = new NetworkPacket
            {
                AddressPart = addressPart,
                Message = "",
                MessageType = MessageTypes.NodeHelloMessage
            };
            return result;
        }

        public static NetworkPacket CreateMGMTHello(AddressPart addressPart)
        {
            NetworkPacket result = new NetworkPacket
            {
                AddressPart = addressPart,
                Message = "",
                MessageType = MessageTypes.MGMTHelloMessage
            };
            return result;
        }

        public static NetworkPacket CreateMGMTMessage(AddressPart addressPart, string message)
        {
            NetworkPacket result = new NetworkPacket
            {
                AddressPart = addressPart,
                Message = message,
                MessageType = MessageTypes.MGMTHelloMessage
            };
            return result;
        }

        public NetworkPacket(SerializationInfo serializationInfo, StreamingContext context)
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
}
