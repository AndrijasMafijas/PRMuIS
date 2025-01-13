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
            byte[] buffer = new byte[1024];
            int a = 0,b = 0;
            // Kreiranje utičnice za prijem datagrama
            Socket recvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);


            // Povezivanje utičnice sa bilo kojom adresom na lokalnom računaru i portom 50000
            IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Any, 27015);
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
                //Console.WriteLine(receivedMessage);
                a = Convert.ToInt32(receivedMessage[0]);
                b = Convert.ToInt32(receivedMessage[2]);
                if (a == 1)
                {
                    Console.WriteLine("Client je odlucio da se koristi TCP");
                }
                else
                {
                    Console.WriteLine("Client je odlucio da se koristi UDP");
                }
                if (b == 1)
                {
                    Console.WriteLine("Client je odlucio da se koristi 3DES");
                }
                else
                {
                    Console.WriteLine("Client je odlucio da se koristi RSA");
                }
                Console.WriteLine("Received {0} bytes from {1}: {2}", bytesReceived,
               senderEndPoint, receivedMessage);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("recvfrom failed with error: {0}", ex.Message);
            }
            // Zatvaranje utičnice nakon prijema
            recvSocket.Close();


            if (a == 1)
            {
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);

                serverSocket.Bind(serverEP);

                serverSocket.Listen(5);


                Console.WriteLine($"Server je stavljen u stanje osluskivanja i ocekuje komunikaciju na {serverEP}");

                Socket acceptedSocket = serverSocket.Accept();

                IPEndPoint clientEP = acceptedSocket.RemoteEndPoint as IPEndPoint;
                Console.WriteLine($"Povezao se novi klijent! Njegova adresa je {clientEP}");



                while (true)
                {
                    try
                    {
                        int brBajta = acceptedSocket.Receive(buffer);
                        if (brBajta == 0)
                        {
                            Console.WriteLine("Klijent je zavrsio sa radom");
                            break;
                        }
                        string poruka = Encoding.UTF8.GetString(buffer,0,brBajta);
                        Console.WriteLine(poruka);


                        if (poruka == "kraj")
                            break;


                        Console.WriteLine("Unesite poruku");
                        string odgovor = Console.ReadLine();

                        brBajta = acceptedSocket.Send(Encoding.UTF8.GetBytes(odgovor));
                        if (odgovor == "kraj")
                            break;
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Doslo je do greske {ex}");
                        break;
                    }

                }

                Console.WriteLine("Server zavrsava sa radom");
                Console.ReadKey();
                acceptedSocket.Close();
                serverSocket.Close();

            }
            else
            {
                // Kreiranje utičnice za slanje podataka
                
                Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                ProtocolType.Udp);


                // Podešavanje adrese primaoca
                recvEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.7"), 50000);
                senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

                sendSocket.Bind(recvEndPoint);

                Console.WriteLine("SenderEP: {0} recvEP: {1}", senderEndPoint, recvEndPoint);

                while (true)
                {
                    
                    try
                    {
                        int bytes = sendSocket.ReceiveFrom(buffer, ref senderEndPoint);

                        if (bytes == 0)
                        {
                            Console.WriteLine("Klijent je zavrsio sa radom");
                            break;
                        }

                        Console.WriteLine("Klijent:");
                        string odgovor = Encoding.UTF8.GetString(buffer,0,bytes);
                        Console.WriteLine(odgovor);
                        if (odgovor == "kraj")
                            break;

                        // Poruka za slanje
                        Console.WriteLine("Unesite poruku za klijenta:");
                        string message = Console.ReadLine();
                        byte[] messageBytes = Encoding.UTF8.GetBytes(message);


                        // Slanje datagrama

                        bytes = sendSocket.SendTo(messageBytes, 0, messageBytes.Length,
                        SocketFlags.None, senderEndPoint);

                        if (message == "kraj")
                            break;
                        Console.WriteLine("Poslato {0} bajtova na {1}", bytes, senderEndPoint);


                        
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Doslo je do greske tokom slanja:\n{ex}");
                        break;
                    }
                    
                }
                // Zatvaranje utičnice nakon slanja
                Console.WriteLine("Server zavrsava sa radom");
                sendSocket.Close();
                Console.ReadKey();
            }
        }
    }
}
