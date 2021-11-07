using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Client
{
    class Client
    {
        private string name;
        private const int initialPort = 8360;
        private int portOffset;
        private Socket clientSocket;

        private object inRoomLock;
        private Thread ReaderThread;
        private Thread WriterThread;

        public Client()
        {
            Console.WriteLine("Welcome to the chat rooms.");
            Console.Write("-> ");
            this.name = Console.ReadLine().Trim();
            this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.portOffset = 0;
        }

        private void ConnectTo(int portNum)
        {
            if (clientSocket.Connected)
            {
                clientSocket.Close();
            }
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(ip, portNum);
            clientSocket.Connect(endPoint);
        }

        public void Run()
        {
            while (true)
            {
                Console.Write("-> ");
                string request = Console.ReadLine();
                byte[] message = new byte[21], uName, rName;
                int start = 0;
                if (request.Length > 5 && request.Substring(0, 5).Equals("join ")) { message[0] = 0; start = 5; }
                else if (request.Length > 7 && request.Substring(0, 7).Equals("create ")) { message[0] = 1; start = 7; }
                else { return; }
                string room = request.Substring(start);
                if (name.Length >= 10) { uName = Encoding.UTF8.GetBytes(name.Substring(0, 10)); } else { uName = Encoding.UTF8.GetBytes(name + new string('\n', 10 - name.Length)); }
                if (room.Length >= 10) { rName = Encoding.UTF8.GetBytes(room.Substring(0, 10)); } else { rName = Encoding.UTF8.GetBytes(room + new string('\n', 10 - room.Length)); }

                Console.WriteLine("Tring to connect to room '" + room + "'");

                uName.CopyTo(message, 1);
                rName.CopyTo(message, 11);

                ConnectTo(initialPort);
                clientSocket.Send(message);

                byte[] response = new byte[4];
                clientSocket.Receive(response);
                portOffset = BitConverter.ToInt32(response);

                JoinRoom();
            }
        }

        private byte[] StringToBytes(string str)
        {
            byte[] encodedLength = BitConverter.GetBytes(str.Length);
            byte[] encoded = Encoding.UTF8.GetBytes(str);
            byte[] bytes = new byte[4 + encoded.Length];

            encodedLength.CopyTo(bytes, 0);
            encoded.CopyTo(bytes, 4);

            return bytes;
        }

        private void JoinRoom()
        {
            ConnectTo(initialPort + portOffset);
            ReaderThread = new Thread(ReaderFunction);
            ReaderThread.Start();
            WriterThread = new Thread(WriterFunction);
            WriterThread.Start();
        }

        static void ReaderFunction()
        {

        }

        static void WriterFunction()
        {

        }
    }
}
