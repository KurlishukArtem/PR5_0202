using System;
using System.Linq;
using System.Text;

namespace Server.Classes
{
    public class Client
    {
        public string Token { get; set; }
        public DateTime DateConnect { get; set; }
        public Client() 
        {
            Random rand = new Random();
            string Chars = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm123456789";
            Token = new string(Enumerable.Repeat(Chars, 15).Select(x => x[rand.Next(Chars.Length)]).ToArray());
            DateConnect = DateTime.Now;

        }

    }
}
