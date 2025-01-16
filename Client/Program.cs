using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Klase; // Pretpostavljam da je klasa TripleDES u ovom namespace-u

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            byte[] buffer = new byte[1024];
            byte[] key = new byte[1024];
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
                neispravan_algoritam:
                Console.WriteLine("Odaberite koje cete sifrfovanje koristiti:");
                Console.WriteLine("Ukoliko zelite 3DES unesite 1 , a ukoliko zelite RSA unesite 2:");
                int b = Convert.ToInt32(Console.ReadLine());

                if (b == 1)
                {
                    neispravan_kljuc:
                    Console.WriteLine("Unesite kljuc za 3DES sifrovanje (NAPOMENA: KLJUC MORA IMATI 24 ZNAKA (BAJTA))!");
                    key =Encoding.UTF8.GetBytes(Console.ReadLine());
                    if(key.Length != 24)
                    {
                        Console.WriteLine("Kljuc mora imati tacno 24 znaka.");
                        goto neispravan_kljuc;
                    }
                }
                else if (b == 2)
                {

                }
                else
                {
                     Console.WriteLine("Neispravno ste uneli algoritam za sifrovanje!");
                    goto neispravan_algoritam;
                }

                

                if ((a == 1 || a == 2) && (b == 1 || b == 2))
                {
                    Socket odabirSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    IPEndPoint odabirEP = new IPEndPoint(IPAddress.Parse(ipaddresa), 27015);
                    string odabir = a.ToString() + " " + b.ToString() + " " + port + " " + Encoding.UTF8.GetString(key);
                    byte[] brojBajtova = Encoding.UTF8.GetBytes(odabir);

                    try
                    {
                        int bytesSent = odabirSocket.SendTo(brojBajtova, 0, brojBajtova.Length, SocketFlags.None, odabirEP);
                        Console.WriteLine("Sent {0} bytes to {1}", bytesSent, odabirEP);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("sendto failed with error: {0}", ex.Message);
                    }
                    odabirSocket.Close();
                }

                if (a == 1)
                {
                    dobar = true;
                    Console.WriteLine("Izabrali ste TCP.");

                    Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(ipaddresa), Convert.ToInt32(port));
                    Console.WriteLine("Klijent je spreman za povezivanje sa serverom, kliknite enter");
                    Console.ReadKey();
                    clientSocket.Connect(serverEP);
                    Console.WriteLine("Klijent je uspesno povezan sa serverom!");

                    while (true)
                    {
                        Console.WriteLine("Unesite poruku za server:");
                        try
                        {
                            string poruka = Console.ReadLine();
                            byte[] encryptedMessage;
                            // Ako je 3DES izabrano, šifrujemo poruku pre slanja
                            if (b == 1)
                            {
                                //byte[] key = Encoding.UTF8.GetBytes("123456789012345678901234"); // primer ključa, treba biti 24 bajta
                                encryptedMessage = TripleDES.Encrypt(poruka, key);
                                clientSocket.Send(encryptedMessage);
                            }
                            else
                            {
                                // Za RSA, dodajte odgovarajuće šifrovanje
                                clientSocket.Send(Encoding.UTF8.GetBytes(poruka));
                            }

                            if (poruka == "kraj")
                                break;

                            int brBajta = clientSocket.Receive(buffer);
                            if (brBajta == 0)
                            {
                                Console.WriteLine("Server je zavrsio sa radom");
                                break;
                            }

                            string odgovor = "";

                            encryptedMessage = new byte[brBajta];
                            Array.Copy(buffer, encryptedMessage, brBajta);

                            // Ako je poruka šifrovana 3DES-om, dešifrujemo je
                            if (b == 1)
                            {
                                odgovor = TripleDES.Decrypt(encryptedMessage, Encoding.UTF8.GetBytes("123456789012345678901234"));
                            }
                            else
                            {
                                odgovor = Encoding.UTF8.GetString(buffer, 0, brBajta);
                            }

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
                    clientSocket.Close();
                }
                else if (a == 2)
                {
                    dobar = true;
                    Console.WriteLine("Izabrali ste UDP.");

                    Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Parse(ipaddresa), Convert.ToInt32(port));
                    EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    while (true)
                    {
                        Console.WriteLine("Unesite poruku za server:");
                        try
                        {
                            string message = Console.ReadLine();
                            byte[] messageBytes;

                            if (b == 1)
                            {
                                //byte[] key = Encoding.UTF8.GetBytes("123456789012345678901234"); // primer ključa, treba biti 24 bajta
                                messageBytes = TripleDES.Encrypt(message, key);
                            }
                            else
                            {
                                messageBytes = Encoding.UTF8.GetBytes(message);
                            }

                            //Console.WriteLine(Convert.ToBase64String(messageBytes));
                            //Console.ReadKey();
                            int bytes = sendSocket.SendTo(messageBytes, 0, messageBytes.Length, SocketFlags.None, recvEndPoint);
                            if (message == "kraj")
                                break;

                            Console.WriteLine("Poslato {0} bajtova na {1}", bytes, recvEndPoint);

                            bytes = sendSocket.ReceiveFrom(buffer, ref senderEndPoint);
                            if (bytes == 0)
                            {
                                Console.WriteLine("Server je zavrsio sa radom");
                                break;
                            }

                            string odgovor;

                            byte[] encryptedMessage = new byte[bytes];
                            Array.Copy(buffer, encryptedMessage, bytes);

                            if (b == 1)
                            {
                                odgovor = TripleDES.Decrypt(encryptedMessage, Encoding.UTF8.GetBytes("123456789012345678901234"));
                            }
                            else
                            {
                                odgovor = Encoding.UTF8.GetString(buffer, 0, bytes);
                            }

                            Console.WriteLine("Server: " + odgovor);
                            if (odgovor == "kraj")
                                break;
                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine($"Doslo je do greske tokom slanja:\n{ex}");
                            break;
                        }
                    }

                    sendSocket.Close();
                }
                else
                {
                    Console.WriteLine("Morate uneti 1 ili 2");
                }
            }
        }
    }
}
