using CableCloud;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;

namespace TSST_MPLS
{
    class CableCloud
    {
        private ManualResetEvent _allDone = new ManualResetEvent(false);
        private Socket _server;
        private readonly IPAddress _cloudAddress;
        private readonly int _cloudPort;
        private readonly CloudPacketProcessor _processor;

        static void Main(string[] args)
        {
            CableCloud cloud = new CableCloud();
            try
            {
                Task.Run(() => cloud.StartCloud());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Press anything to close cloud");
            Console.ReadKey();
        }

        CableCloud()
        {
            _cloudAddress = IPAddress.Parse("127.0.0.1");
            _cloudPort = 62572;
            _processor = new CloudPacketProcessor();
        }

        private void StartCloud()
        {
            _server = new Socket(_cloudAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _server.Bind(new IPEndPoint(_cloudAddress, _cloudPort));
                _server.Listen(100);
                TimeStamp.WriteLine("Cloud suddenly appeared on a blue sky");
                while (true)
                {
                    _allDone.Reset();
                    _server.BeginAccept(new AsyncCallback(AcceptCallback), _server);
                    _allDone.WaitOne();
                }
            }
            catch (Exception e)
            {

            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            _allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            ReceiverState state = new ReceiverState();
            state.WorkSocket = handler;
            handler.BeginReceive(state.Buffer, 0, ReceiverState.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                ReceiverState receiverState = (ReceiverState)ar.AsyncState;
                handler = receiverState.WorkSocket;
                int bytesRead = handler.EndReceive(ar);
                NetworkPacket received = Deserialize(receiverState, bytesRead);
                ProcessorResponse processorResponse = _processor.ProcessPacketAndResponse(handler, received);

                if (processorResponse.socketToSend != null)
                {
                    foreach (Socket socket in processorResponse.socketToSend)
                    {
                        if (socket != null)
                        {
                            _allDone.Reset();
                            Send(socket, processorResponse.networkPacket);
                            _allDone.WaitOne();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: {0}", processorResponse.discardMessage);
                }

                handler.BeginReceive(receiverState.Buffer, 0, receiverState.Buffer.Length, 0, new AsyncCallback(ReadCallback), receiverState);
            }
            catch (SocketException e)
            {
                _processor.DeleteDisconnectedSocket(handler);
            }
        }

        private NetworkPacket Deserialize(ReceiverState receiverState, int byterRead)
        {
            using (var memoryStream = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                memoryStream.Write(receiverState.Buffer, 0, byterRead);
                memoryStream.Seek(0, SeekOrigin.Begin);
                NetworkPacket obj = (NetworkPacket)bf.Deserialize(memoryStream);
                return obj;
            }
        }

        public byte[] SerializeMessage(NetworkPacket networkPacket)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                bf.Serialize(memoryStream, networkPacket);
                return memoryStream.ToArray();
            }
        }

        private void Send(Socket socket, NetworkPacket received)
        {
            try
            {
                ReceiverState state = new ReceiverState();
                state.WorkSocket = socket;
                state.Buffer = SerializeMessage(received);
                socket.BeginSend(state.Buffer, 0, state.Buffer.Length, 0, new AsyncCallback(SendCallback), state);
            }
            catch(SocketException e)
            {
                _processor.DeleteDisconnectedSocket(socket);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                ReceiverState client = (ReceiverState)ar.AsyncState;
                client.WorkSocket.EndSend(ar);
                _allDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
