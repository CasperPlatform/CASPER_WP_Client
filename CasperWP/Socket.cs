using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Xaml.Controls;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace CasperWP
{
    class Socket
    {
        private StreamSocket clientTCPSocket;
        private DatagramSocket clientUDPSocket;
        private HostName serverHost;
        private string serverHostnameString;
        private string serverTCPPort;
        private string serverUDPPort;

        public List<VideoFrame> frames = new List<VideoFrame>();
        public VideoFrame lastFrame;

        private TimeSpan idleTimer = TimeSpan.FromMilliseconds(2500);

        private bool m_TCPConnected = false;

        public bool TCPConnected
        {       
            get { return m_TCPConnected; }
        }

        private bool m_UDPConnected = false;

        public bool UDPConnected
        {
            get { return m_TCPConnected; }
        }

        public Socket(string hostName, string tcpPort, string udpPort)
        {
            serverHostnameString = hostName;
            serverTCPPort = tcpPort;
            serverUDPPort = udpPort;

            clientTCPSocket = new StreamSocket();
            clientUDPSocket = new DatagramSocket();

            clientUDPSocket.MessageReceived += ClientUDPSocket_MessageReceived;
        }

        public event EventHandler ImageCompleted;

        public class ImageCompletedEventArgs : EventArgs
        {
            public byte[] ImageData { get; set; }           
        }

        protected virtual void OnImageCompleted(EventArgs e)
        {
            EventHandler handler = ImageCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public async void TCPConnect()
        {
            if (m_TCPConnected)
            {
                Debug.WriteLine("Already connected to TCP");
                return;
            }

            try
            {
                Debug.WriteLine("Trying to connect to TCP...");

                serverHost = new HostName(serverHostnameString);
                // Try to connect to the 
                await clientTCPSocket.ConnectAsync(serverHost, serverTCPPort);
                m_TCPConnected = true;
                Debug.WriteLine("Connection to TCP established");

            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                Debug.WriteLine("Connect to TCP failed with error: " + exception.Message);
                // Could retry the connection, but for this simple example
                // just close the socket.

                // the Close method is mapped to the C# Dispose
                clientTCPSocket.Dispose();
                clientTCPSocket = null;
            }
        }

        public async void UDPConnect()
        {
            if (m_UDPConnected)
            {
                Debug.WriteLine("Already connected to UDP");
                return;
            }

            try
            {
                Debug.WriteLine("Trying to connect to UDP...");

                serverHost = new HostName(serverHostnameString);
                // Try to connect to the 
                await clientUDPSocket.ConnectAsync(serverHost, serverUDPPort);
                m_UDPConnected = true;
                Debug.WriteLine("Connection to UDP established");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                Debug.WriteLine("Connect to UDP failed with error: " + exception.Message);
                // Could retry the connection, but for this simple example
                // just close the socket.

                // the Close method is mapped to the C# Dispose
                clientUDPSocket.Dispose();
                clientUDPSocket = null;
            }
        }

        public async void SendMessage(Byte[] message)
        {
            if (!m_TCPConnected)
            {
                Debug.WriteLine("Must be connected to TCP to send!");
                return;
            }

            try
            {
                //Debug.WriteLine("Trying to send data over TCP...");

                DataWriter writer = new DataWriter(clientTCPSocket.OutputStream);

                // Call StoreAsync method to store the data to a backing stream
                writer.WriteBytes(message);

                await writer.StoreAsync();

                //Debug.WriteLine("Data was sent over TCP");

                // detach the stream and close it
                writer.DetachStream();
                writer.Dispose();
            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                Debug.WriteLine("Send data or receive over TCP failed with error: " + exception.Message);
                // Could retry the connection, but for this simple example
                // just close the socket.

                clientTCPSocket.Dispose();
                clientTCPSocket = null;
                m_TCPConnected = false;
            }
        }

        public async void StartVideo(Image image)
        {
            string token = "632e81da5c5d8cf2";
            byte[] array = Encoding.UTF8.GetBytes(token);

            byte[] message = new Byte[20];

            message[0] = 0x01;

            Array.Copy(array, 0, message, 1, 16);

            message[17] = (byte)'S';
            message[18] = 0x0D;
            message[19] = 0x0A;

            if (!m_UDPConnected)
            {
                Debug.WriteLine("Must be connected to UDP to send!");
                return;
            }

            try
            {
                Debug.WriteLine("Trying to send data over UDP ...");

                DataWriter writer = new DataWriter(clientUDPSocket.OutputStream);

                // Call StoreAsync method to store the data to a backing stream
                writer.WriteBytes(message);

                await writer.StoreAsync();

                Debug.WriteLine("Data was sent over UDP");         


                // detach the stream and close it
                writer.DetachStream();
                writer.Dispose();

                ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer((IAsyncAction) => IdleDelegate(), idleTimer);
            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                Debug.WriteLine("Send data or receive over UDP failed with error: " + exception.Message);
                // Could retry the connection, but for this simple example
                // just close the socket.

                clientUDPSocket.Dispose();
                clientUDPSocket = null;
                m_UDPConnected = false;
            }
        }

        private async void IdleDelegate()
        {
            string token = "632e81da5c5d8cf2";
            byte[] array = Encoding.UTF8.GetBytes(token);

            byte[] message = new Byte[20];

            message[0] = 0x01;

            Array.Copy(array, 0, message, 1, 16);

            message[17] = (byte)'I';
            message[18] = 0x0D;
            message[19] = 0x0A;

            if (!m_UDPConnected)
            {
                Debug.WriteLine("Must be connected to UDP to send!");
                return;
            }

            try
            {
                Debug.WriteLine("Trying to send data over UDP ...");

                DataWriter writer = new DataWriter(clientUDPSocket.OutputStream);

                // Call StoreAsync method to store the data to a backing stream
                writer.WriteBytes(message);

                await writer.StoreAsync();

                Debug.WriteLine("Data was sent over UDP");


                // detach the stream and close it
                writer.DetachStream();
                writer.Dispose();
            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                Debug.WriteLine("Send data or receive over UDP failed with error: " + exception.Message);
                // Could retry the connection, but for this simple example
                // just close the socket.

                clientUDPSocket.Dispose();
                clientUDPSocket = null;
                m_UDPConnected = false;
            }
        }

        private void ClientUDPSocket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            DataReader reader = args.GetDataReader();
            Byte[] packet = new Byte[reader.UnconsumedBufferLength];

            reader.ReadBytes(packet);

            ReadMessage(packet);
        }

        private void ReadMessage(byte[] message)
        {
            if (message[0] == 0x01 && message[1] == 'V')
            {
                int imageNumber = message[2] << 24 | message[3] << 16 | message[4] << 8 | message[5];

                int packetCount = message[6];

                int imageSize = message[7] << 24 | message[8] << 16 | message[9] << 8 | message[10];

                Debug.WriteLine("New Image received, " + imageNumber + ", " + packetCount + ", " + imageSize);

                frames.Add(new VideoFrame(imageNumber, packetCount, imageSize));
            }
            if (message[0] == 0x02)
            {
                int imageNumber = message[1] << 24 | message[2] << 16 | message[3] << 8 | message[4];
                int packetNumber = message[5];

                for (int i = 0; i < frames.Count; i++)
                {                  
                    if (frames[i].imageNumber == imageNumber)
                    {                        
                        if (frames[i].AddImagePart(message, packetNumber))
                        {
                            Debug.WriteLine("Image completed");
                            lastFrame = frames[i];
                            OnImageCompleted(EventArgs.Empty);

                            for (int j = 0; j < frames.Count; j++)
                            {
                                if (frames[j].imageNumber < lastFrame.imageNumber)
                                {
                                    frames.RemoveAt(j);
                                    j--;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
