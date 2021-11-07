using System;

namespace Server
{
    class ServerRunner
    {
        static void Main(string[] args)
        {
            Server s = new Server();
            s.Run();
        }
    }
}
