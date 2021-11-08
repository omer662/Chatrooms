using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class RoomServer
    {
        private string name;

        private int portNum;
        private IPAddress address;
        private TcpListener listener;



        public RoomServer(string name, int portNum)
        {
            this.name = name;
            this.portNum = portNum;
            address = IPAddress.Any;

            listener = new TcpListener(address, portNum);
        }

        public string GetName()
        {
            return this.name;
        }

        public int GetPortNum()
        {
            return this.portNum;
        }

        public void Run()
        {
            listener.Start();
            Console.WriteLine("Listener for server {0} started, waiting for messages");
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                byte[] data = new byte[266], nameBytes = new byte[10], messageBytes = new byte[256];
                stream.Read(data, 0, 10); // Read Name
                stream.Read(data, 10, 256); // Read message
                stream.Close();
                client.Close();
                Array.Copy(data, 0, nameBytes, 0, 10);
                Array.Copy(data, 10, messageBytes, 0, data.Length - 10);
                string name = Encoding.UTF8.GetString(nameBytes).Trim(), message = Encoding.UTF8.GetString(messageBytes).Trim();
                for (int i = 0; i < name.Length; i++) { if ((byte)name[i] == 0) { name = name.Substring(0, i); break; } }
                for (int i = 0; i < message.Length; i++) { if ((byte)message[i] == 0) { message = message.Substring(0, i); break; } }
                Console.WriteLine("{0}: {1}", name, message);
            }
        }
    }
}
