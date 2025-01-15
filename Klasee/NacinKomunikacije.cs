using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Klase
{
    public class NacinKomunikacije
    {
        
        private Socket clientSocket {  get; set; }
        private string Algoritam {  get; set; }
        private string Kljuc {  get; set; }

        private EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);
        public NacinKomunikacije(int x, string algoritam, string kljuc,EndPoint sep)
        {
            if (x == 1)
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                    ProtocolType.Udp);
            }
            Algoritam = algoritam;
            Kljuc = kljuc;
            senderEndPoint = sep;
        }
    }
}
