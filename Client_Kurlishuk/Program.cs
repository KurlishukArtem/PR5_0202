using System;
using System.IO;
using System.Net;
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
            Console.WriteLine("Hello World!");
        }

        static void OnSettings()
        {
            string Path = Directory.GetCurrentDirectory() + "/.config";
            if (File.Exists(Path))
            {
                StreamReader streamReader = new StreamReader(Path);
                ServerIpAddress = IPAddress.Parse(streamReader.ReadLine());
            }
            else
            {

            }
        }
    }
}
