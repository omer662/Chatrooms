using System;

namespace Client
{
    class ClientRunner
    {
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                ClientListener cl = new ClientListener();
                cl.Run();
            }
            else
            {
                ClientWriter client = new ClientWriter();
                client.Run();
            }
        }


    }
}
