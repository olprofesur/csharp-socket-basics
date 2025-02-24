using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SynchronousSocketClient
{

    public static void StartClient()
    {
        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024];

        // Connect to a remote device.  
        try
        {
            string data = "";
            // Establish the remote endpoint for the socket.  
            // This example uses port 11000 on the local computer.  
            IPAddress ipAddress = System.Net.IPAddress.Parse("127.0.0.1");
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 5000);

            // Create a TCP/IP  socket.  
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}",
                sender.RemoteEndPoint.ToString());

                Command c = new Command();
                c.CommandName = "getServerName";

                String s= Newtonsoft.Json.JsonConvert.SerializeObject(c)+"\n";
                byte[] msg = Encoding.ASCII.GetBytes(s);              //("This is a test<EOF>");

                // Send the data through the socket.  
                int bytesSent = sender.Send(msg);
                data = "";
                // Receive the response from the remote device.  
                do
                {
                    int bytesRec = sender.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                } while (data.IndexOf("\n") == -1);
                Console.WriteLine("Messaggio ricevuto: " + data);
                Response response = Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(data);
                Console.WriteLine("Risposta nell'oggetto: "+response.Result);

                // Release the socket.
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
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

    public static int Main(String[] args)
    {
        StartClient();
        Console.WriteLine("Premere un tasto per continuare ");
        Console.ReadLine();
        return 0;
    }
}
