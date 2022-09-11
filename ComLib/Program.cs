using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLib
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            Console.ReadKey();
        }
    }

    class Server : ServerBehavior
    {
        public Server()
        {
            Start();
        }

        protected override void Started()
        {
            Console.WriteLine("Server started.");
        }

        protected override void Stoped()
        {
            Console.WriteLine("Server stoped.");
        }

        protected override void Joined(Client client)
        {
            Console.WriteLine($"Client-{client.id} joined.");
        }

        protected override void Disconnected(Client client)
        {
            Console.WriteLine($"Client-{client.id} disconnected.");
        }

        protected override void Sent(byte[] data,Client client)
        {
            Console.WriteLine($"Server >> Client-{client.id} : \"{Encoding.Unicode.GetString(data)}\"");
        }

        protected override void Read(byte[] data,Client client)
        {
            Console.WriteLine($"Client-{client.id} > \"{Encoding.Unicode.GetString(data)}\"");
        }
    }
}
