using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Klase; // Pretpostavljam da je klasa u ovom namespace-u

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string port = "";
            string sifrovanje = "";
            //byte[] key = null;
            byte[] buffer = new byte[1024];
            int a = 0, b = 0;
            byte[] key = Encoding.UTF8.GetBytes("123456789012345678901234"); // 24-byte key for 3DES

            // Kreiranje utičnice za prijem datagrama preko postojeće klase
            IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Any, 27015);
            NacinKomunikacije serverCommunication = new NacinKomunikacije(0, "", "", recvEndPoint);

            Console.WriteLine("Ocekujem poruku na " + recvEndPoint);
            try
            {
                // Prijem datagrama
                EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int bytesReceived = serverCommunication.Receive(buffer, ref senderEndPoint);

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                a = Convert.ToInt32(receivedMessage[0]) - 48;
                b = Convert.ToInt32(receivedMessage[2]) - 48;

                string[] delovi = receivedMessage.Split(' ');
                port = delovi[2];
                key = Encoding.UTF8.GetBytes(delovi[3]);
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
                    sifrovanje = "3DES";
                }
                else
                {
                    Console.WriteLine("Client je odlucio da se koristi RSA");
                    sifrovanje = "RSA";
                }
                Console.WriteLine("Received {0} bytes from {1}: {2}", bytesReceived, senderEndPoint, receivedMessage);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("recvfrom failed with error: {0}", ex.Message);
            }

            if (a == 1) // TCP komunikacija
            {
                NacinKomunikacije tcpClient = new NacinKomunikacije(1, sifrovanje, Encoding.UTF8.GetString(key), new IPEndPoint(IPAddress.Any, Convert.ToInt32(port)));

                tcpClient.Listen();
                Console.WriteLine("Server je stavljen u stanje osluskivanja");

                // Prihvatanje nove konekcije
                Socket acceptedSocket = tcpClient.Accept();
                Console.WriteLine($"Povezao se novi klijent! Njegova adresa je {acceptedSocket.RemoteEndPoint}");

                // Komunikacija sa klijentom preko prihvaćenog socket-a
                while (true)
                {
                    try
                    {
                        // Koristi acceptedSocket za prijem podataka
                        int bytesReceived = acceptedSocket.Receive(buffer);
                        if (bytesReceived == 0)
                        {
                            Console.WriteLine("Klijent je zavrsio sa radom");
                            break;
                        }

                        string poruka = "";

                        byte[] encryptedMessage = new byte[bytesReceived];
                        Array.Copy(buffer, encryptedMessage, bytesReceived);
                        // Ako je šifrovanje 3DES, dešifrujemo poruku
                        if (sifrovanje == "3DES")
                        {
                            poruka = TripleDES.Decrypt(encryptedMessage, Encoding.UTF8.GetBytes(tcpClient.Kljuc)); // Dešifrujemo poruku sa 3DES
                        }

                        Console.WriteLine("Klijent: " + poruka);

                        if (poruka == "kraj")
                            break;

                        Console.WriteLine("Unesite poruku");
                        string odgovor = Console.ReadLine();

                        // Ako je šifrovanje 3DES, šifrujemo poruku
                        byte[] encryptedResponse = buffer;
                        if (sifrovanje == "3DES")
                        {
                            encryptedResponse = TripleDES.Encrypt(odgovor, Encoding.UTF8.GetBytes(tcpClient.Kljuc)); // Šifrujemo odgovor sa 3DES
                        }

                        // Koristi acceptedSocket za slanje podataka
                        acceptedSocket.Send(encryptedResponse);
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
                acceptedSocket.Close(); // Zatvori prihvaćeni socket nakon završetka
            }
            else if (a == 2) // UDP komunikacija
            {
                NacinKomunikacije udpClient = new NacinKomunikacije(0, sifrovanje, Encoding.UTF8.GetString(key), new IPEndPoint(IPAddress.Parse("192.168.1.7"), Convert.ToInt32(port)));

                while (true)
                {
                    try
                    {
                        EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        int bytesReceived = udpClient.Receive(buffer, ref senderEndPoint);

                        if (bytesReceived == 0)
                        {
                            Console.WriteLine("Klijent je zavrsio sa radom");
                            break;
                        }

                        // Dešifrovanje poruke ako je potrebno
                        string odgovor = "";
                        //Console.WriteLine(Convert.ToBase64String(buffer));
                        //Console.ReadKey();

                        byte[] encryptedMessage = new byte[bytesReceived];
                        Array.Copy(buffer, encryptedMessage, bytesReceived);

                        if (sifrovanje == "3DES")
                        {
                            odgovor = TripleDES.Decrypt(encryptedMessage,Encoding.UTF8.GetBytes(udpClient.Kljuc)); // Dešifrujemo poruku sa 3DES
                        }

                        Console.WriteLine("Klijent: " + odgovor);

                        if (odgovor == "kraj")
                            break;

                        Console.WriteLine("Unesite poruku za klijenta:");
                        string message = Console.ReadLine();

                        // Šifrovanje poruke ako je potrebno
                        encryptedMessage = Encoding.UTF8.GetBytes(message);
                        if (sifrovanje == "3DES")
                        {
                            encryptedMessage = TripleDES.Encrypt(message, Encoding.UTF8.GetBytes(udpClient.Kljuc)); // Šifrujemo poruku sa 3DES
                        }

                        udpClient.SendTo(encryptedMessage, senderEndPoint);

                        if (message == "kraj")
                            break;
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Doslo je do greske tokom slanja: {ex}");
                        break;
                    }
                }
                Console.WriteLine("Server zavrsava sa radom");
            }
        }
    }
}
