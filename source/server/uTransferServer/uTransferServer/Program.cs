using System;

namespace uTransferServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server _server = new Server(5000);
            _server.Start();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}