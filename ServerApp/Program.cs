using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static TcpListener server;
    static List<TcpClient> clients = new List<TcpClient>();

    static void Main()
    {
        int port = 12345;

        server = new TcpListener(IPAddress.Any, port);
        server.Start();

        Console.WriteLine("Servidor iniciado...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            clients.Add(client);

            Console.WriteLine("Novo cliente conectado!");

            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine(message);

                Broadcast(message, client);
            }
        }
        catch
        {
            Console.WriteLine("Cliente desconectado.");
        }
        finally
        {
            clients.Remove(client);
            client.Close();
        }
    }

    static void Broadcast(string message, TcpClient sender)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        foreach (TcpClient client in clients)
        {
            if (client != sender)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                catch
                {
                    // Ignora erros de envio para clientes desconectados.
                }
            }
        }
    }
}