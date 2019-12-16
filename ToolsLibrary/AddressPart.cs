using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsLibrary
{
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
