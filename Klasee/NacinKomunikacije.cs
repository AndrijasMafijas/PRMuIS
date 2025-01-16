using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Klase
{
    public class NacinKomunikacije
    {
        public Socket clientSocket { get; set; }
        public string Algoritam {  get; set; }
        public string Kljuc {  get; set; }
        public EndPoint senderEndPoint { get; set; }

        public NacinKomunikacije(int x, string algoritam, string kljuc, EndPoint recvEndPoint)
        {
            if (x == 1)
            {
                // TCP socket
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Bind(recvEndPoint);
            }
            else
            {
                // UDP socket
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                clientSocket.Bind(recvEndPoint);
            }
            Algoritam = algoritam;
            Kljuc = kljuc;
            senderEndPoint = recvEndPoint;
        }

        // TCP listen
        public void Listen(int backlog = 5)
        {
            if (clientSocket.ProtocolType == ProtocolType.Tcp)
            {
                clientSocket.Listen(backlog);
            }
            else
            {
                throw new InvalidOperationException("Listen method is not supported for UDP sockets.");
            }
        }

        // TCP accept
        public Socket Accept()
        {
            if (clientSocket.ProtocolType == ProtocolType.Tcp)
            {
                return clientSocket.Accept();
            }
            throw new InvalidOperationException("Accept method is not supported for UDP sockets.");
        }

        // Generic receive (TCP or UDP)
        public int Receive(byte[] buffer, ref EndPoint remoteEP)
        {
            if (clientSocket.ProtocolType == ProtocolType.Tcp)
            {
                return clientSocket.Receive(buffer);
            }
            else
            {
                return clientSocket.ReceiveFrom(buffer, ref remoteEP);
            }
        }

        public int Receive(byte[] buffer)
        {
            if (clientSocket.ProtocolType == ProtocolType.Tcp)
            {
                return clientSocket.Receive(buffer);
            }
            throw new InvalidOperationException("Use the overloaded Receive method for UDP.");
        }

        // Send (TCP)
        public int Send(byte[] buffer)
        {
            if (clientSocket.ProtocolType == ProtocolType.Tcp)
            {
                return clientSocket.Send(buffer);
            }
            throw new InvalidOperationException("Send method is not supported for UDP sockets. Use SendTo instead.");
        }

        // SendTo (UDP)
        public int SendTo(byte[] buffer, EndPoint remoteEP)
        {
            if (clientSocket.SocketType == SocketType.Dgram) // Provera UDP tipa preko SocketType
            {
                return clientSocket.SendTo(buffer, remoteEP);
            }
            throw new InvalidOperationException("SendTo method is not supported for TCP sockets.");
        }

        // Close socket
        public void Close()
        {
            clientSocket.Close();
        }
    }
}