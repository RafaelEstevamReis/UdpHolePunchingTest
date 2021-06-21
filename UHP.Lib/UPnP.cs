using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace UHP.Lib
{
    public class UPnP
    {
        private static readonly string discoverRequest = "M-SEARCH * HTTP/1.1\r\n" +
                                                         "HOST: 239.255.255.250:1900\r\n" +
                                                         "ST:upnp:rootdevice\r\n" +
                                                         "MAN:\"ssdp:discover\"\r\n" +
                                                         "MX:3\r\n\r\n";

        public static TimeSpan TimeOut { get; set; } = new TimeSpan(0, 0, 0, 3);
        public static bool Discover()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            string _serviceUrl;
            string _descUrl;

            byte[] data = Encoding.ASCII.GetBytes(discoverRequest);

            foreach (var ip in SocketHelper.GetAllBroadcastAddresses())
            {
                IPEndPoint ipe = new IPEndPoint(ip, 1900);
                byte[] buffer = new byte[0x1000];

                s.SendTo(data, ipe);

                int length;
                //do
                //{
                Thread.Sleep(500);
                if (s.Available == 0) continue;
                length = s.Receive(buffer);

                string resp = Encoding.ASCII.GetString(buffer, 0, length);
                if (!resp.Contains("upnp:rootdevice")) continue;

                resp = resp.Substring(resp.IndexOf("location:", StringComparison.InvariantCultureIgnoreCase) + 9);
                resp = resp.Substring(0, resp.IndexOf("\r")).Trim();


                if (string.IsNullOrEmpty(_serviceUrl = getServiceUrl(resp))) continue;

                _descUrl = resp;
                return true;

            }

            return false;
        }

        private static string getServiceUrl(string responseLocation)
        {
            string _eventUrl;
            Stream content;

            try
            {
                var request = WebRequest.Create(responseLocation);
                var respose = request.GetResponse();

                content = respose.GetResponseStream();
            }
            catch
            {
                return null;
            }


            XmlDocument desc = new XmlDocument();
            desc.Load(content);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(desc.NameTable);
            nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");

            XmlNode typen = desc.SelectSingleNode("//tns:device/tns:deviceType/text()", nsMgr);
            if (!typen.Value.Contains("InternetGatewayDevice")) return null;

            XmlNode node = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:controlURL/text()", nsMgr);
            if (node == null) return null;

            return new Uri(new Uri(responseLocation), node.Value).ToString();

        }

        public static void SendForwardPort(int port, ProtocolType protocol, string description, string serviceUrl)
        {
            string content = "<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
                "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + port.ToString() + "</NewExternalPort><NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
                "<NewInternalPort>" + port.ToString() + "</NewInternalPort><NewInternalClient>" + Dns.GetHostAddresses(Dns.GetHostName())[0].ToString() +
                "</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>" + description +
                "</NewPortMappingDescription><NewLeaseDuration>0</NewLeaseDuration></u:AddPortMapping>";

            executeSoapRequest(serviceUrl, content, "AddPortMapping");
        }

        public static void DeleteForwardingRule(int port, ProtocolType protocol, string serviceUrl)
        {
            string content = "<u:DeletePortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
                "<NewRemoteHost>" +
                "</NewRemoteHost>" +
                "<NewExternalPort>" + port + "</NewExternalPort>" +
                "<NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
                "</u:DeletePortMapping>";

            executeSoapRequest(serviceUrl, content, "DeletePortMapping");
        }

        public static IPAddress GetExternalIP(string serviceUrl)
        {
            if (string.IsNullOrEmpty(serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");

            XmlDocument xdoc = executeSoapRequest(serviceUrl, "<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
            "</u:GetExternalIPAddress>", "GetExternalIPAddress");
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
            nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
            string IP = xdoc.SelectSingleNode("//NewExternalIPAddress/text()", nsMgr).Value;
            return IPAddress.Parse(IP);
        }

        private static XmlDocument executeSoapRequest(string url, string soap, string function)
        {
            string req = "<?xml version=\"1.0\"?>" +
            "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
            "<s:Body>" +
            soap +
            "</s:Body>" +
            "</s:Envelope>";

            byte[] data = Encoding.UTF8.GetBytes(req);
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            request.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:service:WANIPConnection:1#" + function + "\"");
            request.ContentType = "text/xml; charset=\"utf-8\"";
            request.ContentLength = data.Length;

            var requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            var response = request.GetResponse(); ;

            XmlDocument responseContent = new XmlDocument();
            Stream ress = response.GetResponseStream();
            responseContent.Load(ress);
            return responseContent;
        }
    }
}