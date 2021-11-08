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
        private TcpClient client;

        public Client()
        {
            Console.Write("Name: ");
            this.name = Console.ReadLine().Trim();
        }

        public void Run()
        {
            while (true)
            {
                this.client = new TcpClient();

                Console.Write("Connect to: ");
                string rName = Console.ReadLine().Trim();

                if (rName.Equals("EXIT")) { break; }

                byte[] request = new byte[20], nameBytes = new byte[10], roomBytes = new byte[10];

                Encoding.UTF8.GetBytes(name).CopyTo(request, 0);
                Encoding.UTF8.GetBytes(rName).CopyTo(request, 10);

                client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8360));
                NetworkStream stream = client.GetStream();
                stream.Write(request);

                byte[] newPort = new byte[4];
                stream.Read(newPort);

                stream.Close();
                client.Close();

                while (true)
                {
                    Console.Write("{0}> ", name);
                    string message = Console.ReadLine().Trim();

                    if(message.Equals("EXIT")) { break; }
                    if (message.Length > 256) { message = message.Substring(0, 256); }
                    byte[] fullMessage = new byte[266], messageBytes = Encoding.UTF8.GetBytes(message);
                    Encoding.UTF8.GetBytes(name).CopyTo(fullMessage, 0);
                    Array.Copy(messageBytes, 0, fullMessage, 10, messageBytes.Length);

                    client = new TcpClient();
                    client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), BitConverter.ToInt32(newPort)));
                    NetworkStream ns = client.GetStream();
                    ns.Write(fullMessage, 0, fullMessage.Length);
                    ns.Close();
                    client.Close();
                }
            }
        }

    }
}
