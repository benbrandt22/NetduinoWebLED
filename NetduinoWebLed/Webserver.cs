using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Reflection;

namespace NetduinoWebLed
{
    public class WebServer : IDisposable
    {
        
        private Socket socket = null;
        //open connection to onbaord led so we can blink it with every request
        private OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        public WebServer()
        {
            //Initialize Socket class
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Request and bind to an IP from DHCP server

            IPAddress ip = IPAddress.Parse(Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress);
            IPEndPoint endpoint = new IPEndPoint(ip, 80);
            socket.Bind(endpoint);

            //Debug print our IP address
            Debug.Print(endpoint.Address.ToString());

            //Start listen for web requests
            socket.Listen(10);
            ListenForRequest();
        }

        public void ListenForRequest()
        {
            while (true)
            {
                using (Socket clientSocket = socket.Accept())
                {
                    //Get clients IP
                    IPEndPoint clientIP = clientSocket.RemoteEndPoint as IPEndPoint;
                    EndPoint clientEndPoint = clientSocket.RemoteEndPoint;
                    //int byteCount = cSocket.Available;
                    int bytesReceived = clientSocket.Available;
                    if (bytesReceived > 0)
                    {
                        //Get request
                        byte[] buffer = new byte[bytesReceived];
                        int byteCount = clientSocket.Receive(buffer, bytesReceived, SocketFlags.None);
                        string request = new string(Encoding.UTF8.GetChars(buffer));
                        Debug.Print(request);
                        //Compose a response
                        var response = ProcessWebRequest(request);
                        clientSocket.Send(Encoding.UTF8.GetBytes(response.Header), response.Header.Length, SocketFlags.None);
                        clientSocket.Send(Encoding.UTF8.GetBytes(response.Content), response.Content.Length, SocketFlags.None);
                        
                    }
                }
            }
        }
        #region IDisposable Members
        ~WebServer()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (socket != null)
                socket.Close();
        }
        #endregion

        private WebResponse ProcessWebRequest(string request)
        {
            var response = new WebResponse { HttpStatusCode = 200 };


            if (request != null)
            {
                

                request = request.ToLower();
                if (request.IndexOf("led=on") > 0)
                {
                    led.Write(true);
                    response = new WebResponse { HttpStatusCode = 303, RedirectUrl = "/" };
                }
                else if (request.IndexOf("led=off") > 0)
                {
                    led.Write(false);
                    response = new WebResponse { HttpStatusCode = 303, RedirectUrl = "/" };
                }
                else {
                    response.Content = GetStatusPage();
                }
            }
            else
            {
                response.Content = "";
            }
            return response;
        }

        private string GetStatusPage() {
            var ledState = led.Read();

            var page = new StringBuilder();
            page.AppendLine("<html>");

            page.AppendLine("<head>");
            page.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            page.AppendLine("<title>Netduino Web-Enabled LED</title>");
            page.AppendLine("</head>");

            page.AppendLine("<body>");
            page.AppendLine("<p>The LED is " + (ledState ? "ON" : "OFF") + "</p>");
            page.AppendLine("<p><a href=\"/?led=off\">Turn off</a></p>");
            page.AppendLine("<p><a href=\"/?led=on\">Turn on</a></p>");
            page.AppendLine("</body>");
            
            page.AppendLine("</html>");

            return page.ToString();
        }

    }

    public class WebResponse {
        private string content;


        public int HttpStatusCode { get; set; }
        public string Header {
            get
            {
                if (HttpStatusCode == 200) {
                    return "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " +
                           Content.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                }
                if (HttpStatusCode == 303) {
                    return "HTTP/1.0 303 See Other\r\nLocation:" + RedirectUrl;
                }
                return "";
            }
        }

        public string RedirectUrl { get; set; }

        public string Content
        {
            get { return content ?? ""; }
            set { content = value; }
        }
    }

}
