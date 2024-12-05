using System;
using System.ComponentModel.Design;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Client_Kurlishuk
{
    internal class Program
    {
        static IPAddress ServerIpAddress;
        static int ServerPort;

        static string ClientToken;
        static DateTime ClientDateConnection;

        static void Main(string[] args)
        {
            OnSettings();
            Thread tCheckToken = new Thread(CheckToken);
            tCheckToken.Start();
            while (true) { SetCommand(); }
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
            else if (Command == "/connect") ConnectServer();
            else if (Command == "/status") GetStatus();
            else if (Command == "/help") Help();
        }

        static void ConnectServer() 
        {
            IPEndPoint EndPoint = new IPEndPoint(ServerIpAddress, ServerPort);
            Socket Socket = new Socket(
                AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Socket.Connect(EndPoint);
            }
            catch (Exception ex) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
            }
            if (Socket.Connected) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Connection to server sucsessful: ");

                Socket.Send(Encoding.UTF8.GetBytes("/token"));


                byte[] Bytes = new byte[10485760];
                int ByteRec = Socket.Receive(Bytes);

                string Response = Encoding.UTF8.GetString(Bytes, 0, ByteRec);

                if (Response == "/limit")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("not enoght free space on server: ");
                }
                else {
                    ClientToken = Response;
                    ClientDateConnection = DateTime.Now;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Recieved connection token: " + ClientToken);
                }
            }
        }

        static void CheckToken()
        {
            while (true) { 
                if(!String.IsNullOrEmpty(ClientToken))
                {
                    IPEndPoint EndPoint = new IPEndPoint(ServerIpAddress, ServerPort);
                    Socket Socket = new Socket(
                        AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        Socket.Connect(EndPoint);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    if (Socket.Connected)
                    {
                       

                        Socket.Send(Encoding.UTF8.GetBytes(ClientToken));


                        byte[] Bytes = new byte[10485760];
                        int ByteRec = Socket.Receive(Bytes);

                        string Response = Encoding.UTF8.GetString(Bytes, 0, ByteRec);

                        if (Response == "/disconnect")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The client is disconnected from server: ");
                            ClientToken = string.Empty;
                        }
                        
                    }
                }
                Thread.Sleep(1000);
            }
            
        }

        static void GetStatus()
        {
            int Duration = (int)DateTime.Now.Subtract(ClientDateConnection).TotalSeconds;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Client: {ClientToken}. Time Connection: {ClientDateConnection.ToString("HH:mm:ss dd.MM")}, " + 
                $"Duration: {Duration}");
        }

        static void Help() {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Commands to the server: ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/config: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("- set initial settings");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/connect: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("- connection to the server");

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
                streamReader.Close();

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("server address: ");
                Console.ForegroundColor= ConsoleColor.Green;
                Console.WriteLine(IpAddress);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("server port: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(ServerPort.ToString());
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

                StreamWriter streamWriter = new StreamWriter(Path);
                streamWriter.WriteLine(IpAddress);
                streamWriter.WriteLine(ServerPort.ToString());
                streamWriter.Close();
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("ToChange, write the command");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/config");
        }
    }
}
