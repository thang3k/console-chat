using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server = Classes.Program.ChatServer;
namespace server
{
    internal class Program
    {
        static void Main(string[] args)
        {
           Server server = new Server();
            server.Start();


        }
    }
}
