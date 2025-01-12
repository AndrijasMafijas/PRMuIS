using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
       
                    // Kreiranje utičnice za slanje podataka
                    Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                    ProtocolType.Udp);


                    // Podešavanje adrese primaoca
                    IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.7"), 50000);


                    // Poruka za slanje
                    string message = "Veza je uspostavljena. Port je 50000 i udp je protokol.";
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                    // Slanje datagrama
                    try
                    {
                        int bytesSent = sendSocket.SendTo(messageBytes, 0, messageBytes.Length,
                       SocketFlags.None, recvEndPoint);
                        Console.WriteLine("Sent {0} bytes to {1}", bytesSent, recvEndPoint);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("sendto failed with error: {0}", ex.Message);
                    }
                    // Zatvaranje utičnice nakon slanja
                    sendSocket.Close();
                
                Console.ReadKey();
            
        }
    }
}
