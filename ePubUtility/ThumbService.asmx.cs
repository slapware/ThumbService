using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
//
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Configuration;
using System.Threading;

namespace ePubUtility
{
    /// <summary>
    /// Summary description for ThumbService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class ThumbService : System.Web.Services.WebService
    {

        [WebMethod(Description = "Send command to window service via TCP.", EnableSession = false)]
        public string SendCommand(string pCmnd)
        {
            Int32 dPort;
            dPort = System.Convert.ToInt32(ConfigurationManager.AppSettings["DestPort"]);
            string destHost;
            destHost = ConfigurationManager.AppSettings["EServerName"];
            IPHostEntry ipHostEnt;
            try
            {
                ipHostEnt = Dns.GetHostEntry(destHost);
            }
            catch (System.Exception ex)
            {
                string err = "Exception Thrown: " + ex.ToString();
                return err;
            }

            IPEndPoint ipEnd = new IPEndPoint(ipHostEnt.AddressList[0], dPort);
            //            Socket s = null;

            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer 
            // connected to the same address as specified by the server, port
            // combination.
            try
            {
                TcpClient client = new TcpClient(AddressFamily.InterNetwork);
                client.Connect(destHost, dPort);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(pCmnd);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                //    Console.WriteLine("Sent: {0}", message);         

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[32];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                try
                {
                    if (stream.CanRead)
                    {
                        StringBuilder myCompleteMessage = new StringBuilder();
                        int numberOfBytesRead = 0;

                        // Incoming message may be larger than the buffer size.
                        do
                        {
                            numberOfBytesRead = stream.Read(data, 0, data.Length);
                            myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(data, 0, numberOfBytesRead));
                        }
                        while (stream.DataAvailable);
                    }
                }
                catch (System.Exception ex)
                {
                    string serr = ex.Message;
                    return serr;
                }
                //    Console.WriteLine("Received: {0}", responseData);         

                // Close everything.
                stream.Close();
                client.Close();
                return responseData;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            return @"done";
        }
        [WebMethod(Description = "Dam Jacket Cover generation request.", EnableSession = false)]
        public string MakeThumb(string pCmnd)
        {
            string destHost;
            destHost = ConfigurationManager.AppSettings["EServerName"];
            Int32 dPort;
            dPort = System.Convert.ToInt32(ConfigurationManager.AppSettings["DestPort"]);
            IPHostEntry lipa = Dns.GetHostEntry(destHost);
            IPEndPoint lep = new IPEndPoint(lipa.AddressList[0], dPort);

            Socket s = new Socket(lep.Address.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);
            try
            {
                s.Connect(lep);
            }
            catch (Exception e)
            {
                string err = "Exception Thrown: " + e.ToString();
                return err;
            }

            byte[] msg = Encoding.ASCII.GetBytes(pCmnd);
            /******************************
            * Blocks until send returns.  *
            ******************************/
            int i = s.Send(msg, SocketFlags.None);
            /******************************
            * Blocks until read returns.  *
            ******************************/
            byte[] bytes = new byte[1024];
            s.Receive(bytes, SocketFlags.None);
            /*************************
            * Displays to the user.  *
            *************************/
            string dret = Encoding.ASCII.GetString(bytes);
            s.Shutdown(SocketShutdown.Both);
            s.Close();

            return dret;
        }
        [WebMethod(Description = "Send command to window service via TCP.", EnableSession = false)]
        public string SendCommand2(string pCmnd)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                Int32 dPort;
                dPort = System.Convert.ToInt32(ConfigurationManager.AppSettings["DestPort"]);
                string destHost;
                destHost = ConfigurationManager.AppSettings["EServerName"];
                tcpClient.Connect(destHost, dPort);
                NetworkStream networkStream = tcpClient.GetStream();

                if (networkStream.CanWrite && networkStream.CanRead)
                {

                    // Does a simple write.
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(pCmnd);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);

                    // Reads the NetworkStream into a byte buffer.
                    byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
                    networkStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);

                    // Returns the data received from the host to the console.
                    string returndata = Encoding.ASCII.GetString(bytes);
                    Console.WriteLine("This is what the host returned to you: " + returndata);

                    return returndata;
                }
                else if (!networkStream.CanRead)
                {
                    string returndata = @"You can not write data to this stream";
                    tcpClient.Close();
                }
                else if (!networkStream.CanWrite)
                {
                    string returndata = @"You can not read data from this stream";
                    tcpClient.Close();
                }
                return @"bad result";
            }
            catch (Exception e)
            {
                string returndata = (e.ToString());
            }
            return @"bad data";
        }
    }
}