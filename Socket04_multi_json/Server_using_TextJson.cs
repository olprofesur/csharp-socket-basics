using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

public class SynchronousSocketListener
{
    internal static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void StartListening()
    {
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 5000);

        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Console.WriteLine("Receive Timeout (ms): {0}", listener.ReceiveTimeout);

        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();

                var clientThread = new ClientManager(handler);
                var t = new Thread(new ThreadStart(clientThread.DoClient));
                t.Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.ReadLine();
    }

    public static int Main(string[] args)
    {
        StartListening();
        return 0;
    }
}

public class ClientManager
{
    private readonly Socket _clientSocket;
    private readonly byte[] _buffer = new byte[1024];
    private string _data = "";

    public ClientManager(Socket clientSocket) => _clientSocket = clientSocket;

    public void DoClient()
    {
        Command? lastCmd = null;

        try
        {
            do
            {
                _data = "";
                while (_data.IndexOf("\n", StringComparison.Ordinal) == -1)
                {
                    int bytesRec = _clientSocket.Receive(_buffer);
                    if (bytesRec == 0) break;
                    _data += Encoding.ASCII.GetString(_buffer, 0, bytesRec);
                }
                if (string.IsNullOrWhiteSpace(_data)) break;

                string jsonLine = _data.TrimEnd('\r', '\n');

                // Deserializzo in object e poi casto a Command
                object? objAny = JsonSerializer.Deserialize(jsonLine, typeof(Command), SynchronousSocketListener.JsonOptions);

                if (objAny is Command cmd)
                {
                    lastCmd = cmd;
                    Console.WriteLine("Messaggio ricevuto (Command): {0}", jsonLine);
                    Console.WriteLine($"Cast OK -> CommandName={cmd.CommandName}, ParamCount={cmd.ParametersList?.Count ?? 0}");
                }
                else
                {
                    Console.WriteLine("Payload non compatibile con il tipo Command.");
                    break;
                }

                var response = new Response { Result = "Hi from the server" };
                string responseJson = JsonSerializer.Serialize(response, SynchronousSocketListener.JsonOptions) + "\n";
                byte[] msg = Encoding.ASCII.GetBytes(responseJson);
                _clientSocket.Send(msg);

            } while (lastCmd is not null && !string.Equals(lastCmd.CommandName, "quit", StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            try { _clientSocket.Shutdown(SocketShutdown.Both); } catch { }
            _clientSocket.Close();
        }
    }

    public class Command
    {
        public string? CommandName { get; set; }
        public List<string>? ParametersList { get; set; }
    }

    public class Response
    {
        public string? Result { get; set; }
    }
}
