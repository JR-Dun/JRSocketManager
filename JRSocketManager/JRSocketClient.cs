using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace JRSocketManager
{
    class JRSocketClient
    {
        private static string _ip = "127.0.0.1";
        // 监听端口     
        private static int _port = 2601;
        // ManualResetEvent instances signal completion.     
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        // The response from the remote device.     
        private static String response = String.Empty;
        private static Socket client;

        public static event SocketInfo socketInfo;
        public delegate void SocketInfo(string info);

        public static void ConfigPara(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public static void StartVideo(string courseId)
        {
            // Connect to a remote device.     
            try
            {
                IPAddress ipAddress = IPAddress.Parse(_ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);
                // Create a TCP/IP socket.     
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the remote endpoint.     
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();
                // Send test data to the remote device.  
                StringBuilder sb = new StringBuilder();
                sb.Append("{\"key\":\"1\",\"content\":\"");
                sb.Append(courseId);
                sb.Append("\"}");
                Send(client, sb.ToString());
                sendDone.WaitOne();
                // Receive the response from the remote device.     
                Receive(client);
                receiveDone.WaitOne();
                // Write the response to the console.     
                sb = new StringBuilder();
                sb.Append("Response received : ");
                sb.Append(response);
                printInfo(sb.ToString());
                // Release the socket.     
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static void StopVideo(string courseId)
        {
            // Connect to a remote device.     
            try
            {
                IPAddress ipAddress = IPAddress.Parse(_ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);
                // Create a TCP/IP socket.     
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the remote endpoint.     
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();
                // Send test data to the remote device. 
                StringBuilder sb = new StringBuilder();
                sb.Append("{\"key\":\"2\",\"content\":\"");
                sb.Append(courseId);
                sb.Append("\"}");
                Send(client, sb.ToString());
                sendDone.WaitOne();
                // Receive the response from the remote device.     
                Receive(client);
                receiveDone.WaitOne();
                // Write the response to the console.     
                sb = new StringBuilder();
                sb.Append("Response received : ");
                sb.Append(response);
                printInfo(sb.ToString());
                // Release the socket.     
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void StartClient()
        {
            // Connect to a remote device.     
            try
            {
                IPAddress ipAddress = IPAddress.Parse(_ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);
                // Create a TCP/IP socket.     
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the remote endpoint.     
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();
                // Send test data to the remote device.     
                Send(client, "This is a test<EOF>");
                sendDone.WaitOne();
                // Receive the response from the remote device.     
                Receive(client);
                receiveDone.WaitOne();
                // Write the response to the console.     
                StringBuilder sb = new StringBuilder();
                sb.Append("Response received : ");
                sb.Append(response);
                printInfo(sb.ToString());
                // Release the socket.     
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.     
                Socket client = (Socket)ar.AsyncState;
                // Complete the connection.     
                client.EndConnect(ar);
                StringBuilder sb = new StringBuilder();
                sb.Append("Socket connected to ");
                sb.Append(client.RemoteEndPoint.ToString());
                printInfo(sb.ToString());
                // Signal that the connection has been made.     
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.     
                JRStateObject state = new JRStateObject();
                state.workSocket = client;
                // Begin receiving the data from the remote device.     
                client.BeginReceive(state.buffer, 0, JRStateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket     
                // from the asynchronous state object.     
                JRStateObject state = (JRStateObject)ar.AsyncState;
                Socket client = state.workSocket;
                // Read data from the remote device.     
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.     

                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    // Get the rest of the data.     
                    client.BeginReceive(state.buffer, 0, JRStateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.     
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.     
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.     
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            // Begin sending the data to the remote device.     
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.     
                Socket client = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.     
                int bytesSent = client.EndSend(ar);
                StringBuilder sb = new StringBuilder();
                sb.Append("Sent ");
                sb.Append(bytesSent);
                sb.Append(" bytes to client.");
                printInfo(sb.ToString());
                // Signal that all bytes have been sent.     
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void printInfo(string info)
        {
            Console.WriteLine(info);
            socketInfo(info);
        }
    }
}
