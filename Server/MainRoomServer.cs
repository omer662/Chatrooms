using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace Server
{
    class MainRoomServer
    {
        private int roomNum = 0;

        private int portNum;
        private IPAddress address;
        private TcpListener listener;

        private List<RoomServer> rooms;

        public MainRoomServer()
        {
            portNum = 8360;
            address = IPAddress.Any;
            listener = new TcpListener(address, portNum);
            rooms = new List<RoomServer>();
        }

        private (RoomServer, int, bool) GetRoom(string name) /// (RoomServer - new/one that already exists, RoomServer's port number, Is the RoomServer new)
        {
            int pn = 8360;
            bool searching = true;
            foreach (var r in rooms)
            {
                pn++;
                if (r.GetName() == name)
                {
                    return (r, r.GetPortNum(), false);
                }
                if (searching && pn != r.GetPortNum())
                {
                    searching = false;
                }
            }
            if (searching) { pn++; }
            roomNum++;
            return (new RoomServer(name, pn), pn, true);
        }

        public void Run()
        {
            listener.Start();
            while (true)
            {
                Console.WriteLine("Waiting for a new connection");
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Got new connection from {0}", client.Client.RemoteEndPoint);
                NetworkStream stream = client.GetStream();

                byte[] request = new byte[20], nameBytes = new byte[10], roomBytes = new byte[10];
                stream.Read(request, 0, request.Length);

                Array.Copy(request, 0, nameBytes, 0, 10); // Get user name or SERVEREXIT message
                Array.Copy(request, 10, roomBytes, 0, 10);

                string uName = Encoding.UTF8.GetString(nameBytes).Trim(), rName = Encoding.UTF8.GetString(roomBytes).Trim();
                for (int i = 0; i < uName.Length; i++) { if ((byte)uName[i] == 0) { uName = uName.Substring(0, i); break; } }
                for (int i = 0; i < rName.Length; i++) { if ((byte)rName[i] == 0) { rName = rName.Substring(0, i); break; } }
                Console.WriteLine("Redirected user '{0}' to room '{1}'", uName, rName);
                (RoomServer requestedRoom, int port, bool startProcess) = GetRoom(rName);

                if (uName.Equals("SERVEREXIT"))
                {
                    rooms.Remove(requestedRoom);
                    Console.WriteLine("Server {0} closed.", rName);
                    continue;
                }
                rooms.Add(requestedRoom);

                stream.Write(BitConverter.GetBytes(port)); 
                if (!startProcess) { continue; }
                ProcessStartInfo psi = new ProcessStartInfo("Server.exe")
                {
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    Arguments = requestedRoom.GetName() + " " + port.ToString()
                };
                Process.Start(psi);
            }
        }
    }
}
