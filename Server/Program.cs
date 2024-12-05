using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Server.Classes;

namespace Server
{
    public class Program
    {
        static IPAddress ServerIpAddress;
        static int ServerPort;
        static int MaxClient;
        static int Duration;

        static List<Classes.Client> AllClients = new List<Classes.Client>();
        static void Main(string[] args)
        {
            OnSettings();

            Thread tLisener = new Thread(ConnectServer);
            tLisener.Start();

            Thread tDisconnect = new Thread(CheckDisconnectClient);
            tDisconnect.Start();

            while (true)
            {
                SetCommand();
            }
        }

        static void CheckDisconnectClient()
        {
            while (true)
            {
                for (int iClients = 0; iClients < AllClients.Count; iClients++)
                {
                    int ClientDuration = (int)DateTime.Now.Subtract(AllClients[iClients].DateConnect).TotalSeconds;

                    if (ClientDuration > Duration)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Client:  {AllClients[iClients].Token} disconnect from server due timeout");
                        AllClients.RemoveAt(iClients);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        static void SetCommand()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            string Command = Console.ReadLine();
            if (Command == "/config")
            {
                File.Delete(Directory.GetCurrentDirectory() + "/.config");
                OnSettings();

            }
            else if (Command.Contains("/disconnect")) DisconnnectServer(Command);
            else if (Command == "/status") GetStatus();
            else if (Command == "/help") Help();
        }

        static string SetCommandClient(string Command)
        {
            if (Command == "/token")
            {
                if (AllClients.Count < MaxClient)
                {
                    Classes.Client newClient = new Classes.Client();
                    AllClients.Add(newClient);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"new client connection: " + newClient.Token);

                    return newClient.Token;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"there is not enogh free space on the license server");
                    return "/limit";
                }
            }
            else
            {
                Classes.Client Client = AllClients.Find(x => x.Token == Command);
                return Client != null ? "/connect" : "/disconnect";
            }
            return null;
        }

        static void DisconnnectServer(string command) 
        {
            try
            {
                string Token = command.Replace("/disconnect ", "");
                Classes.Client DissconnectClient = AllClients.Find(x => x.Token == Token);
                AllClients.Remove(DissconnectClient);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Client:  {Token} disconnect from server");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: " + ex.Message);
            }
        }

        static void ConnectServer()
        {
            IPEndPoint EndPoint = new IPEndPoint(ServerIpAddress, ServerPort);
            Socket SocketLisener = new Socket(
                AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketLisener.Bind(EndPoint);
            SocketLisener.Listen(10);

            while (true) {
                Socket Handler = SocketLisener.Accept();

                byte[] Bytes = new byte[10485760];
                int ByteRec = Handler.Receive(Bytes);

                string Message = Encoding.UTF8.GetString(Bytes, 0, ByteRec);
                string Response = SetCommandClient(Message);

                Handler.Send(Encoding.UTF8.GetBytes(Response));
            }
        }

        static void GetStatus()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"count users: {AllClients.Count}");

            foreach (Classes.Client client in AllClients)
            {
                int Duration = (int)DateTime.Now.Subtract(client.DateConnect).TotalSeconds;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Client: {client.Token}. Time Connection: {client.DateConnect.ToString("HH:mm:ss dd.MM")}, " +
                    $"Duration: {Duration}");
            }

            
        }

        static void Help()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Commands to the clients: ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/config: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("- set initial settings");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/disconnect: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("- disconnection users from the server");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/status: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("- show list users");
        }

        static void OnSettings()
        {
            string Path = Directory.GetCurrentDirectory() + "/.config";
            string IpAddress = "";
            if (File.Exists(Path))
            {
                StreamReader streamReader = new StreamReader(Path);
                IpAddress = streamReader.ReadLine();
                ServerIpAddress = IPAddress.Parse(IpAddress);
                ServerPort = int.Parse(streamReader.ReadLine());
                MaxClient = int.Parse(streamReader.ReadLine());
                Duration = int.Parse(streamReader.ReadLine());
                streamReader.Close();

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("server address: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(IpAddress);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("server port: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(ServerPort.ToString());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Max count clients: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(MaxClient.ToString());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Token lifetime: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Duration.ToString());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Place provide IP Address: ");
                Console.ForegroundColor = ConsoleColor.Green;
                IpAddress = Console.ReadLine();
                ServerIpAddress = IPAddress.Parse(IpAddress);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Place provide port: ");
                Console.ForegroundColor = ConsoleColor.Green;
                ServerPort = int.Parse(Console.ReadLine());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("please indicate count of clients: ");
                Console.ForegroundColor = ConsoleColor.Green;
                MaxClient = int.Parse(Console.ReadLine());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Specufy the token lifetime: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Duration = int.Parse(Console.ReadLine());

                StreamWriter streamWriter = new StreamWriter(Path);
                streamWriter.WriteLine(IpAddress);
                streamWriter.WriteLine(ServerPort.ToString());
                streamWriter.WriteLine(MaxClient.ToString());
                streamWriter.WriteLine(Duration.ToString());
                streamWriter.Close();
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("ToChange, write the command");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/config");
        }
    }
}
