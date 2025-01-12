using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Odaberite koji cete protokol koristiti:");
            Console.WriteLine("Ukoliko zelite TCP unesite 1 , a ukoliko zelite UDP unesite 2:");
            int a = Convert.ToInt32(Console.ReadLine());
            bool dobar = false;
            while (!dobar)
            {

                if (a == 1)
                {
                    dobar = true;
                    // Kreiranje utičnice za prijem datagrama
                    Socket recvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                    ProtocolType.Udp);


                    // Povezivanje utičnice sa bilo kojom adresom na lokalnom računaru i portom 27015
                    IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Any, 50000);
                    recvSocket.Bind(recvEndPoint);


                    // Bafer za prijem podataka
                    byte[] recvBuf = new byte[1024];
                    EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    try
                    {
                        // Prijem datagrama
                        int bytesReceived = recvSocket.ReceiveFrom(recvBuf, ref senderEndPoint);

                        // Konverzija primljenih bajtova u string
                        string receivedMessage = Encoding.UTF8.GetString(recvBuf, 0, bytesReceived);
                        Console.WriteLine("Received {0} bytes from {1}: {2}", bytesReceived,
                       senderEndPoint, receivedMessage);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("recvfrom failed with error: {0}", ex.Message);
                    }
                    // Zatvaranje utičnice nakon prijema
                    recvSocket.Close();
                    Console.ReadKey();
                }
                else if (a == 2)
                {
                    dobar = true;
                }
                else
                {
                    Console.WriteLine("Morate izabrati 1 ili 2!");
                }
            }
            Console.ReadKey();
        }
    }
}
