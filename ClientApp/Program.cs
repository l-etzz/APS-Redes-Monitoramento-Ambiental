
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== SISTEMA DE MONITORAMENTO AMBIENTAL ===");
        Console.WriteLine();

        Console.Write("Digite seu nome: ");
        string nome = Console.ReadLine() ?? "";

        Console.Write("Digite sua estação: ");
        string estacao = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(nome) ||
            string.IsNullOrWhiteSpace(estacao))
        {
            Console.WriteLine("Nome e estação são obrigatórios.");
            return;
        }

        string serverIP = "127.0.0.1";
        int port = 12345;

        try
        {
            using TcpClient client = new TcpClient(serverIP, port);
            using NetworkStream stream = client.GetStream();

            Console.WriteLine();
            Console.WriteLine("Conectado ao servidor!");
            Console.WriteLine($"Identificação: {nome} | {estacao}");
            Console.WriteLine("Digite suas mensagens abaixo.");
            Console.WriteLine("Digite /sair para desconectar.");
            Console.WriteLine();

            Thread receiveThread = new Thread(() =>
            {
                byte[] buffer = new byte[1024];

                try
                {
                    while (true)
                    {
                        int bytesRead = stream.Read(
                            buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                            break;

                        string message = Encoding.UTF8.GetString(
                            buffer, 0, bytesRead);

                        Console.WriteLine();
                        Console.WriteLine(message);
                        Console.Write("> ");
                    }
                }
                catch
                {
                    // A conexão foi encerrada.
                }
            });

            receiveThread.IsBackground = true;
            receiveThread.Start();

            while (true)
            {
                Console.Write("> ");
                string mensagem = Console.ReadLine() ?? "";

                if (mensagem.Equals(
                    "/sair",
                    StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(mensagem))
                    continue;

                string mensagemIdentificada =
                    $"[{nome} | {estacao}] {mensagem}";

                byte[] data = Encoding.UTF8.GetBytes(
                    mensagemIdentificada);

                stream.Write(data, 0, data.Length);
            }
        }
        catch (SocketException)
        {
            Console.WriteLine(
                "Não foi possível conectar ao servidor. " +
                "Verifique se ele está em execução.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }

        Console.WriteLine("Cliente encerrado.");
    }
}
