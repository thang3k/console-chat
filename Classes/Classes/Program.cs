using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Microsoft.SqlServer.Server;
using System.Data.SqlClient;
using System.Data;

namespace Classes
{
    public class Program
    {
        static void Main(string[] args)
        {

        }

        public class User
        {
            public string Username { get; set; }
            public bool IsOnline { get; set; }
            public List<Message> Messages { get; set; }

            public User()
            {

            }
            public User(string usr,bool is_on, List<Message>mes)
            {
                this.Username = usr;
                this.IsOnline = is_on;
                this.Messages = mes;
            }
            public User(string usr, string pwd, SqlConnection conn)
            {
                User user = new User();

                try
                {
                    String query = "SELECT a.username, b.ChatRoomName FROM _USERs a JOIN ChatMembers c ON a.idUser = c.UserID JOIN GroupChats b ON b.ChatRoomID = c.ChatRoomID WHERE a.username = '"+usr+"' and a.pwd='"+pwd+"'";
                    SqlDataAdapter sad = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    sad.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        user.Username =dt.Rows[2].ToString();
                        user.Messages.
                    }
                }
                catch (SqlException se)
                {

                    Console.WriteLine(se);
                }
                finally {
                    string query1 = "UPDATE _USERs SET is_online =1 WHERE username='" + user.Username+ "'";
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                    user.IsOnline = true;
                    conn.Close();
                }
                
            }
        }

        public class Message
        {
            public string Sender { get; set; }
            public string Recipient { get; set; }
            public string Content { get; set; }
            public DateTime Timestamp { get; set; }


        }

        public class GroupChat
        {
            public string Name { get; set; }
            public List<User> Members { get; set; }
            public List<Message> Messages { get; set; }
        }


        public class ChatServer
        {
            private List<ClientHandler> clients= new List<ClientHandler>();
            private TcpListener _listener;
            private User clientLock= new User();
            public void Start()
            {
                String IP = null;
                Int32 port = 2009;
                var host = Dns.GetHostByName(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.ToString().Contains('.'))
                    {
                        IP = ip.ToString();
                    }
                }
                _listener= new TcpListener(IPAddress.Parse(IP),port);
                TcpClient client= _listener.AcceptTcpClient();
                Console.WriteLine($"Server started at {IP} on port {port}");
                while (true)
                {
                    TcpClient tcpClient = _listener.AcceptTcpClient();
                    Console.WriteLine("New client connected");
                    ClientHandler clienthandler = new ClientHandler(tcpClient, this);
                    Thread clientThread = new Thread(clienthandler.HandleClient);
                    clientThread.Start();
                }

            }
            public void AddClient(ClientHandler client)
            {
                lock (clientLock)
                {
                    clients.Add(client);
                }
            }
            public void RemoveClient(ClientHandler client)
            {
                lock (clientLock)
                {
                    clients.Remove(client);
                }
            }
            public void BroadcastMessage(string message, string sender)
            {
                lock (clientLock)
                {
                    foreach (var client in clients)
                    {
                        client.SendMessage($"{sender}: {message}");
                    }
                }
            }
        }
        public class ClientHandler
        {
            private User _user;
            private TcpClient client;
            private StreamReader reader;
            private StreamWriter writer;
            private ChatServer server;

            public ClientHandler(TcpClient client, ChatServer server)
            {
                this.client = client;
                this.server = server;

                reader = new StreamReader(client.GetStream());
                writer = new StreamWriter(client.GetStream());
                server.AddClient(this);
            }
            public void HandleClient()
            {
                try
                {
                   _user.Username = reader.ReadLine();
                    Console.WriteLine($"User {_user.Username} connected.");

                    while (true)
                    {
                      string  message = reader.ReadLine();
                        if (string.IsNullOrEmpty(message))
                        {
                            break; // Client disconnected
                        }
                        server.BroadcastMessage(message, _user.Username);
                    }
                }
                catch (IOException)
                {
                    // Handle client disconnect
                }
                finally
                {
                    server.RemoveClient(this);
                    Console.WriteLine("Client disconnected.");
                }
            }

            public void SendMessage(string message)
            {
                writer.WriteLine(message);
                writer.Flush();
            }
        }
                }
}
