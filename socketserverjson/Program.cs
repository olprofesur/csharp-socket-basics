using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

public class SynchronousSocketListener
{

    // Incoming data from the client.  
    public static string data = null;

    public static void StartListening()
    {


        // Establish the local endpoint for the socket.  
        // Dns.GetHostName returns the name of the   
        // host running the application.  
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 5000);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        Console.WriteLine("Timeout : {0}", listener.ReceiveTimeout);

        // Bind the socket to the local endpoint and   
        // listen for incoming connections.  
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.  
            while (true)
            {

                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.  
                Socket handler = listener.Accept();

                ClientManager clientThread = new ClientManager(handler);
                Thread t = new Thread(new ThreadStart(clientThread.doClient));
                t.Start();

            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

    }

    public static int Main(String[] args)
    {
        StartListening();
        return 0;
    }


}

public class ClientManager
{

    Socket clientSocket;
    byte[] bytes = new Byte[1024];
    String data = "";

    public ClientManager(Socket clientSocket)
    {
        this.clientSocket = clientSocket;
    }

    public void doClient()
    {
        Command obj;
        try
        {
            do
            {
                // An incoming connection needs to be processed.  
                data = "";
                do
                {
                    int bytesRec = clientSocket.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                } while (data.IndexOf("\n") == -1);

                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Command>(data);

                // Show the data on the console.  
                Console.WriteLine("Messaggio ricevuto : {0}", data);

                // Echo the data back to the client...
                // as response Message
                Response r = new Response();
                r.Result = "Hi from the server";
                string response= Newtonsoft.Json.JsonConvert.SerializeObject(r)+"\n";
                byte[] msg = Encoding.ASCII.GetBytes(response);

                clientSocket.Send(msg);

            } while (obj != null && obj.CommandName != "quit");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }

    class Command
    {
        public string CommandName { get; set; }
        public List<String> ParametersList { get; set; }
    }

    class Response
    {
        public string Result { get; set; }
    }
}
