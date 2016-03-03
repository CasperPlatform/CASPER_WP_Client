using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace CasperWP
{
    class Socket
    {
        private StreamSocket clientSocket;
        private HostName serverHost;
        private string serverHostnameString;
        private string serverPort;

        private bool m_connected = false;

        public bool Connected
        {       
            get { return m_connected; }
        }

        private bool closing = false;

        public Socket(string hostName, string port)
        {
            serverHostnameString = hostName;
            serverPort = port;

            clientSocket = new StreamSocket();
        }

        public async void Connect()
        {
            if (m_connected)
            {
                Debug.WriteLine("Already connected");
                return;
            }

            try
            {
                Debug.WriteLine("Trying to connect ...");

                serverHost = new HostName(serverHostnameString);
                // Try to connect to the 
                await clientSocket.ConnectAsync(serverHost, serverPort);
                m_connected = true;
                Debug.WriteLine("Connection established");

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

                Debug.WriteLine("Connect failed with error: " + exception.Message);
                // Could retry the connection, but for this simple example
                // just close the socket.

                closing = true;
                // the Close method is mapped to the C# Dispose
                clientSocket.Dispose();
                clientSocket = null;
            }
        }

        public async void SendMessage(Byte[] message)
        {
            if (!m_connected)
            {
                Debug.WriteLine("Must be connected to send!");
                return;
            }

            try
            {
                Debug.WriteLine("Trying to send data ...");

                DataWriter writer = new DataWriter(clientSocket.OutputStream);

                // Call StoreAsync method to store the data to a backing stream
                writer.WriteBytes(message);

                await writer.StoreAsync();

                Debug.WriteLine("Data was sent");

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

                Debug.WriteLine("Send data or receive failed with error: " + exception.Message);
                // Could retry the connection, but for this simple example
                // just close the socket.

                closing = true;
                clientSocket.Dispose();
                clientSocket = null;
                m_connected = false;
            }
        }
    }
}
