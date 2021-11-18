using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class RoomServer
    {
        private string name;

        private int portNum;
        private IPAddress address;
        private TcpListener listener;

        private List<string> names;
        private List<string> clientIps;

        public RoomServer(string name, int portNum)
        {
            this.name = name;
            this.portNum = portNum;
            address = IPAddress.Any;
            names = new List<string>();
            clientIps = new List<string>();

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
            Console.WriteLine("Listener for server '{0}' started at port {1}, waiting for messages", name, portNum);
            while (true)
            {
                bool leave = false;
                int waitedFor = 0; // Adds seconds up to 300 (5 minutes)
                while (!listener.Pending())
                {
                    if (waitedFor == 300) { leave = true; break; }
                    Thread.Sleep(1000);
                    waitedFor++;
                }
                if (leave) { 
                    Console.WriteLine("Closing Server");
                    TcpClient tempClient = new TcpClient();
                    tempClient.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8360));
                    NetworkStream ns = tempClient.GetStream();
                    byte[] buffer = new byte[20];
                    Array.Copy(Encoding.UTF8.GetBytes("SERVEREXIT"), 0, buffer, 0, 10);
                    Array.Copy(Encoding.UTF8.GetBytes(this.name), 0, buffer, 10, Encoding.UTF8.GetBytes(this.name).Length);
                    ns.Write(buffer, 0, buffer.Length);
                    ns.Close();
                    tempClient.Close();
                    break;
                }
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                byte[] data = new byte[266], nameBytes = new byte[10], messageBytes = new byte[256];
                stream.Read(data, 0, 10); // Read Name
                stream.Read(data, 10, 256); // Read message
                string clientIP = client.Client.RemoteEndPoint.ToString();
                clientIP = clientIP.Substring(0, clientIP.IndexOf(':'));
                stream.Close();
                client.Close();
                Array.Copy(data, 0, nameBytes, 0, 10);
                Array.Copy(data, 10, messageBytes, 0, data.Length - 10);
                string name = Encoding.UTF8.GetString(nameBytes).Trim(), message = Encoding.UTF8.GetString(messageBytes).Trim();
                for (int i = 0; i < name.Length; i++) { if ((byte)name[i] == 0) { name = name.Substring(0, i); break; } }
                for (int i = 0; i < message.Length; i++) { if ((byte)message[i] == 0) { message = message.Substring(0, i); break; } }
                if (!names.Contains(name)) 
                { 
                    names.Add(name);
                    clientIps.Add(clientIP);
                    Console.WriteLine("User {0} has entered the room.", name); 
                }
                if (message.Equals("EXIT"))
                {
                    int i = names.IndexOf(name);
                    names.Remove(name);
                    clientIps.RemoveAt(i);
                    Console.WriteLine("User {0} has left the room.", name);
                }
                else
                {
                    Console.WriteLine("{0}: {1}", name, message);
                }

                // Send message to all clients
                foreach (var ip in clientIps)
                {
                    TcpClient tempClient = new TcpClient();
                    tempClient.Connect(new IPEndPoint(IPAddress.Parse(ip), 8350));
                    NetworkStream ns = tempClient.GetStream();
                    ns.Write(data, 0, data.Length);
                    ns.Close();
                    tempClient.Close();
                }
            }
        }
    }
}
