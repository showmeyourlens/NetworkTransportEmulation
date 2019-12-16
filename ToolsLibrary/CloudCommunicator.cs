using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace ToolsLibrary
{
    public abstract class CloudCommunicator
    {
        private readonly IPAddress instanceAddress;
        public int instancePort;
        private Socket cloudSocket;
        private Socket clientSocket;
        public ManualResetEvent sendDone;
        private readonly int cloudPort;

        public CloudCommunicator(string instancePort)
        {
            this.instanceAddress = IPAddress.Parse("127.0.0.1");
            this.cloudPort = 62572;
            this.instancePort = Int32.Parse(instancePort);
            this.sendDone = new ManualResetEvent(false);
        }
        public void Start()
        {
            clientSocket = new Socket(instanceAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Bind(new IPEndPoint(instanceAddress, instancePort));
                clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), cloudPort), new AsyncCallback(ConnectCallback), clientSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Stop()
        {
            cloudSocket.Disconnect(false);
            clientSocket.Disconnect(false);
            clientSocket.Close();
            cloudSocket.Close();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                ReceiverState state = new ReceiverState();
                state.WorkSocket = (Socket)ar.AsyncState;
                cloudSocket = state.WorkSocket;
                cloudSocket.EndConnect(ar);

                // Sending HELLO message to cloud
                Send(CreateHelloMessage());

                Task.Run(() => Receive(cloudSocket));
            }
            catch (ObjectDisposedException e0)
            {
                Console.WriteLine(e0);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }

        private byte[] SerializeMessage(NetworkPacket networkPacket)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using var memoryStream = new MemoryStream();
            bf.Serialize(memoryStream, networkPacket);
            return memoryStream.ToArray();
        }

        private NetworkPacket DeserializeMessage(ReceiverState receiverState, int byteRead)
        {
            using var memoryStream = new MemoryStream();
            var bf = new BinaryFormatter();
            memoryStream.Write(receiverState.Buffer, 0, byteRead);
            memoryStream.Seek(0, SeekOrigin.Begin);
            NetworkPacket obj = (NetworkPacket)bf.Deserialize(memoryStream);
            return obj;
        }

        public void Send(NetworkPacket networkPacket)
        {
            sendDone.Reset();
            ReceiverState state = new ReceiverState();
            state.WorkSocket = clientSocket;
            state.Buffer = SerializeMessage(networkPacket);
            clientSocket.BeginSend(state.Buffer, 0, state.Buffer.Length, 0, new AsyncCallback(SendCallback), state);
            sendDone.WaitOne();
        }

        private void Receive(Socket socket)
        {
            ReceiverState state = new ReceiverState();
            state.WorkSocket = socket;
            socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, new AsyncCallback(ReceiveCallback), state);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                ReceiverState state = (ReceiverState)ar.AsyncState;
                state.WorkSocket.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            ReceiverState receiverState = (ReceiverState)ar.AsyncState;
            try
            {
                int bytesRead = receiverState.WorkSocket.EndReceive(ar);
                NetworkPacket networkPacket = DeserializeMessage(receiverState, bytesRead);
                switch(networkPacket.MessageType)
                {
                    case NetworkPacket.MessageTypes.ClientToClientMessage:
                        ProcessReceivedClientMessage(networkPacket);
                        break;
                    case NetworkPacket.MessageTypes.MGMTMessage:
                        ProcessReceivedManagementMessage(networkPacket);
                        break;
                    case NetworkPacket.MessageTypes.NodeHelloMessage:
                        TimeStamp.WriteLine("Connected to CableCloud");
                        break;
                    default:
                        TimeStamp.WriteLine("Received unrecognised package type. Discarded");
                        break;
                };

                receiverState.WorkSocket.BeginReceive(receiverState.Buffer, 0, receiverState.Buffer.Length, 0, new AsyncCallback(ReceiveCallback), receiverState);
            }
            catch (Exception e)
            {
                TimeStamp.WriteLine("Connection has been lost");
            }
        }

        public abstract NetworkPacket CreateHelloMessage();
        public abstract void ProcessReceivedClientMessage(NetworkPacket networkPacket);
        public abstract void ProcessReceivedManagementMessage(NetworkPacket networkPacket);
    }
}
