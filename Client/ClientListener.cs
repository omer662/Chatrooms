using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class ClientListener
    {
        private TcpListener listener;
        private List<string> names;

        public ClientListener()
        {
            this.listener = new TcpListener(IPAddress.Any, 8350);
            names = new List<string>();
        }

        public void Run()
        {
            listener.Start();

            while (true)
            {
                bool leave = false;
                int waitedFor = 0; // Adds seconds up to 300 (10 minutes)
                while (!listener.Pending())
                {
                    if (waitedFor == 300) { leave = true; break; }
                    Thread.Sleep(1000);
                    waitedFor++;
                }
                if (leave)
                {
                    Console.WriteLine("Closing Server");
                    break;
                }
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
                if (!names.Contains(name)) { 
                    names.Add(name); 
                    Console.WriteLine("User {0} has entered the room.", name); 
                }
                if (message.Equals("EXIT"))
                {
                    names.Remove(name);
                    Console.WriteLine("User {0} has left the room.", name);
                }
                else
                {
                    Console.WriteLine("{0}: {1}", name, message);
                }
            }
        }
    }
}
