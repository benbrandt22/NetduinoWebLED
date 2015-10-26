using System;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace NetduinoWebLed
{
    public class Program
    {
        
        public static void Main() {

            
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();
            WebServer webServer = new WebServer();
            webServer.ListenForRequest();

            Debug.Print("End");

        }

















    }
}
