using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace JRSocketManager
{
    class JRSocketListener
    {
        // 监听端口     
        private const int port = 2601;
        // Thread signal.     
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public static event SocketInfo socketInfo;
        public delegate void SocketInfo(string info);

        public static event SocketOP socketOp;
        public delegate void SocketOP(JRCommand cmd);


        public static void StartListening()
        {    
            byte[] bytes = new Byte[1024];
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {    
                    allDone.Reset();
                    printInfo("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            printInfo("Press ENTER to continue...");
        }
        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.     
            allDone.Set();
            // Get the socket that handles the client request.     
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            // Create the state object.     
            JRStateObject state = new JRStateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, JRStateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }
        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            // Retrieve the state object and the handler socket     
            // from the asynchronous state object.     
            JRStateObject state = (JRStateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            // Read data from the client socket.     
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.     
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                // Check for end-of-file tag. If it is not there, read     
                // more data.     
                content = state.sb.ToString();

                JRCommand cmd = JsonHelper.ParseFromJson<JRCommand>(content);
                socketOp(cmd);

                //if (content.IndexOf("<EOF>") > -1)
                //{
                StringBuilder sb = new StringBuilder();
                    sb.Append("Read ");
                    sb.Append(content.Length);
                    sb.Append(" bytes from socket. \n ");
                    sb.Append("Data : ");
                    sb.Append(content);
                    printInfo(sb.ToString());

                    // Echo the data back to the client.     
                    Send(handler, content);
                //}
                //else
                //{
                //    // Not all data received. Get more.     
                //    handler.BeginReceive(state.buffer, 0, JRStateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                //}
            }
        }
        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.     
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            // Begin sending the data to the remote device.     
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.     
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.     
                int bytesSent = handler.EndSend(ar);
                StringBuilder sb = new StringBuilder();
                sb.Append("Sent ");
                sb.Append(bytesSent);
                sb.Append(" bytes to client.");
                printInfo(sb.ToString());

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
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
