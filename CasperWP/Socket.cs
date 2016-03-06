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

        private int packetCount = 0;
        private int imageSize = 0;
        private int currentPacket = 0;
        private int currentByte = 0;
        private Byte[][] currentImage;
        private bool isFinished = false;

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

        private bool videoStarted = false;
        private Image videoView;

        private bool closing = false;

        public Socket(string hostName, string tcpPort, string udpPort)
        {
            serverHostnameString = hostName;
            serverTCPPort = tcpPort;
            serverUDPPort = udpPort;

            clientTCPSocket = new StreamSocket();
            clientUDPSocket = new DatagramSocket();

            clientUDPSocket.MessageReceived += ClientUDPSocket_MessageReceived;
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

                closing = true;
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

                closing = true;
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

                closing = true;
                clientTCPSocket.Dispose();
                clientTCPSocket = null;
                m_TCPConnected = false;
            }
        }

        public async void StartVideo(Image image)
        {
            Byte[] message = new Byte[1];

            message[0] = (Byte)'D';

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
                videoStarted = true;
                videoView = image;

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

                closing = true;
                clientUDPSocket.Dispose();
                clientUDPSocket = null;
                m_UDPConnected = false;
            }
        }

        private void ClientUDPSocket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            Debug.WriteLine(currentPacket);

            DataReader reader = args.GetDataReader();

            Byte[] message = new Byte[reader.UnconsumedBufferLength];

            reader.ReadBytes(message);            
           
            if(message[0] == 'V' && !isFinished)
            {
                string packetLength = "";
                string imageLength = "";

                for(int i = 1; i<2; i++)
                {
                    packetLength += (char)message[i];
                    Debug.WriteLine(packetLength);
                }

                packetCount = int.Parse(packetLength);
                Debug.WriteLine("Packet Count is: " + packetCount);

                for (int i = 2; i<message.Length; i++)
                {
                    imageLength += (char)message[i]; 
                }

                imageSize = int.Parse(imageLength);
                Debug.WriteLine("Image Size is: " + imageSize);

                currentImage = new Byte[packetCount][];

                currentPacket = 0;
                currentByte = 0;

                isFinished = true;
            }
            else
            {
               
                currentImage[currentPacket] = message;

                currentByte += message.Length;

                currentPacket++;

                Debug.WriteLine("CurrentPacket = " + currentPacket + "/" + packetCount + ", CurrentByte = " + currentByte + "/" + imageSize + ".");

                if (currentPacket == packetCount)
                {
                    Debug.WriteLine(currentByte + ", " + imageSize);

                    Byte[] imageArray = new Byte[imageSize];
                    int currentIndex = 0;

                    foreach (Byte[] array in currentImage)
                    {
                        System.Buffer.BlockCopy(array, 0, imageArray, currentIndex, array.Length);

                        currentIndex += array.Length;
                    }
           
                    Debug.WriteLine("before");

                    ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) => ImageConverter(imageArray)));

                    Debug.WriteLine("last");

                    isFinished = true;
                }
            }
        }

        public async void ImageConverter(Byte[] image)
        {
            Debug.WriteLine("Test1");
            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            Debug.WriteLine("Test2");

            await randomAccessStream.WriteAsync(image.AsBuffer());
            Debug.WriteLine("Test3");

            randomAccessStream.Seek(0); // Just to be sure.
            Debug.WriteLine("Test4");

            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            Debug.WriteLine("Test5");

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                WriteableBitmap bitmap = new WriteableBitmap(640, 480);
                Debug.WriteLine("Test5");

                await bitmap.SetSourceAsync(randomAccessStream);
                Debug.WriteLine("Test6");


                videoView.Source = bitmap;
            }); 
        }
    }
}
