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

            
            byte[] buffer = new byte[1024];
            bool dobar = false;
            while (!dobar)
            {
                Console.WriteLine("Unesite ip adresu na koju zelite da posaljete poruku:");
                string ipaddresa = Console.ReadLine();
                Console.WriteLine("Unesite port na koji zelite da posaljete poruku:");
                string port = Console.ReadLine();
                Console.WriteLine("Odaberite koji cete protokol koristiti:");
                Console.WriteLine("Ukoliko zelite TCP unesite 1 , a ukoliko zelite UDP unesite 2:");
                int a = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Odaberite koje cete sifrfovanje koristiti:");
                Console.WriteLine("Ukoliko zelite 3DES unesite 1 , a ukoliko zelite RSA unesite 2:");
                int b = Convert.ToInt32(Console.ReadLine());
                //ako je dobro uneto salje se serveru
                if ((a == 1 || a == 2) && (b == 1 || b == 2))
                {
                    // Kreiranje utičnice za slanje podataka
                    Socket odabirSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                    ProtocolType.Udp);
                    // Podešavanje adrese primaoca
                    IPEndPoint odabirEP = new IPEndPoint(IPAddress.Parse(ipaddresa), 27015);
                    // Poruka za slanje
                    string odabir = a.ToString() + " " + b.ToString() + " " + port;
                    byte[] brojBajtova = Encoding.UTF8.GetBytes(odabir);

                    // Slanje odabira
                    try
                    {
                        int bytesSent = odabirSocket.SendTo(brojBajtova, 0, brojBajtova.Length,
                       SocketFlags.None, odabirEP);
                        Console.WriteLine("Sent {0} bytes to {1}", bytesSent, odabirEP);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("sendto failed with error: {0}", ex.Message);
                    }
                    // Zatvaranje utičnice nakon slanja
                    odabirSocket.Close();
                }

                if (a == 1)
                {
                    dobar = true;
                    #region PovezivanjeTCP
                    Console.WriteLine("Izabrali ste tcp.");
                    Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(ipaddresa), Convert.ToInt32(port));
                    

                    Console.WriteLine("Klijent je spreman za povezivanje sa serverom, kliknite enter");
                    Console.ReadKey();
                    clientSocket.Connect(serverEP);
                    Console.WriteLine("Klijent je uspesno povezan sa serverom! Protokol je TCP, a port " + port + ".");
                    #endregion

                    #region KomunikacijaTCP
                    while (true)
                    {
                        Console.WriteLine("Unesite poruku za server:");
                        try
                        {
                            string poruka = Console.ReadLine();
                            int brBajta = clientSocket.Send(Encoding.UTF8.GetBytes(poruka));

                            if (poruka == "kraj")
                                break;

                            Console.WriteLine("Poslato {0} bajtova na {1}", brBajta, serverEP);

                            brBajta = clientSocket.Receive(buffer);

                            if (brBajta == 0)
                            {
                                Console.WriteLine("Server je zavrsio sa radom");
                                break;
                            }

                            string odgovor = Encoding.UTF8.GetString(buffer,0,brBajta);

                            Console.WriteLine(odgovor);
                            if (odgovor == "kraj")
                                break;

                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine($"Doslo je do greske tokom slanja:\n{ex}");
                            break;
                        }

                    }
                    #endregion

                    #region ZatvaranjeTCP
                    Console.WriteLine("Klijent zavrsava sa radom");
                    clientSocket.Close();
                    Console.ReadKey();
                    #endregion
                }
                else if (a == 2)
                {
                     dobar = true;
                    Console.WriteLine("Izabrali ste udp.");
                     // Kreiranje utičnice za slanje podataka
                     Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                     ProtocolType.Udp);


                    // Podešavanje adrese primaoca
                    IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Parse(ipaddresa), Convert.ToInt32(port));
                    EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    while (true)
                    {
                        // Poruka za slanje
                        Console.WriteLine("Unesite poruku za server:");
                        try
                        {
                            string message = Console.ReadLine();
                            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                            
                            // Slanje datagrama

                            int bytes = sendSocket.SendTo(messageBytes, 0, messageBytes.Length,
                            SocketFlags.None, recvEndPoint);

                            if (message == "kraj")
                                break;
                            Console.WriteLine("Poslato {0} bajtova na {1}", bytes, recvEndPoint);


                            bytes = sendSocket.ReceiveFrom(buffer, ref senderEndPoint);

                            if (bytes == 0)
                            {
                                Console.WriteLine("Server je zavrsio sa radom");
                                break;
                            }

                            Console.WriteLine("Server:");
                            string odgovor = Encoding.UTF8.GetString(buffer , 0,bytes);
                            Console.WriteLine(odgovor);
                            if (odgovor == "kraj")
                                break;

                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine($"Doslo je do greske tokom slanja:\n{ex}");
                            break;
                        }
                        
                    }

                    // Zatvaranje utičnice nakon slanja
                    Console.WriteLine("Klijent zavrsava sa radom");
                    sendSocket.Close();
                    Console.ReadKey();

                }
                else
                {
                    Console.WriteLine("Morate uneti 1 ili 2");
                }
            }
        }
    }
}
