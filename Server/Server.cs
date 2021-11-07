using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Server
    {
        private const int initialPort = 8360;

        private List<string> roomNames;
        private List<int> roomClientNums;
        private List<Thread> roomHandlers;

        private List<(string, string)> clients; //(uName, rName)

        private TcpListener mainListener;

        public Server()
        {
            mainListener = new TcpListener(IPAddress.Parse("127.0.0.1"), initialPort);
            roomNames = new List<string>();
            roomClientNums = new List<int>();
            clients = new List<(string, string)>();
            roomHandlers = new List<Thread>();
        }

        public void Run()
        {
            mainListener.Start();
            while (true)
            {
                Console.WriteLine("Waiting for connections");
                TcpClient newClient = mainListener.AcceptTcpClient();
                Console.WriteLine("Got new connection from " + newClient.Client.RemoteEndPoint.ToString());

                NetworkStream stream = newClient.GetStream();
                byte[] bytes = new byte[21];
                stream.Read(bytes, 0, bytes.Length);

                byte[] uNameBytes = new byte[10];
                byte[] rNameBytes = new byte[10];

                Array.Copy(bytes, 1, uNameBytes, 0, 10);
                Array.Copy(bytes, 11, rNameBytes, 0, 10);
                //bytes.CopyTo(rNameBytes, 10);

                string uName = Encoding.UTF8.GetString(uNameBytes).Trim();
                string rName = Encoding.UTF8.GetString(rNameBytes).Trim();

                Console.WriteLine(roomNames.ToArray().ToString());

                if (bytes[0] == 0) //Connect
                {
                    if (roomNames.Contains(rName))
                    {
                        Console.WriteLine("Connected user '" + uName + "' to room '" + rName + "'");
                        clients.Add((uName, rName));
                        roomClientNums[roomNames.IndexOf(rName)] += 1;
                        newClient.Client.Send(BitConverter.GetBytes(roomNames.IndexOf(rName) + 1)); //Let the user know the port offset
                    }
                    else
                    {
                        Console.WriteLine("User '" + uName + "' tried to connect to non-existent room '" + rName + "'");
                        newClient.Client.Send(BitConverter.GetBytes(0));
                    }
                }
                else //Initiate
                {
                    if (roomNames.Contains(rName))
                    {
                        Console.WriteLine("User '" + uName + "' tried to create existent room '" + rName + "'");
                        newClient.Client.Send(BitConverter.GetBytes(0));
                    }
                    else
                    {
                        int num = -1;
                        for (int i=0; i < roomNames.Count; i++) 
                        { 
                            if (roomNames[i].Equals("")) 
                            { 
                                roomNames[i] = rName;
                                roomHandlers[i] = new Thread(new ParameterizedThreadStart(HandleRoom));
                                num = i + 1;
                                break; 
                            } 
                        }
                        if (num == -1) {
                            num = roomNames.Count;
                            roomNames.Add(rName);
                            roomClientNums.Add(0);
                            roomHandlers.Add(new Thread(new ParameterizedThreadStart(HandleRoom)));
                            num++;
                        }
                        Console.WriteLine("User '" + uName + "' created room '" + rName + "' with number " + num);
                        newClient.Client.Send(BitConverter.GetBytes(num));

                        roomHandlers[num-1].Start(num);
                    }
                }

                newClient.Close();
            }
        }

        static void HandleRoom(object po)
        {
            int portOffset = (int)po;
            Console.WriteLine("Starting handle of room number {0}", portOffset);
            TcpListener host = new TcpListener(IPAddress.Parse("127.0.0.1"), initialPort + portOffset);
        }
    }
}
