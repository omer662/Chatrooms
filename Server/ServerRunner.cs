using System;

namespace Server
{
    class ServerRunner
    {
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                if (args[0] != string.Empty && args[1] != string.Empty)
                {
                    RoomServer rs = new RoomServer(args[0], int.Parse(args[1]));
                    rs.Run();
                }
            }
            else
            {
                MainRoomServer s = new MainRoomServer();
                s.Run();
            }
        }
    }
}
