using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VCCS.UI
{
    public class SIPSoftPhoneState
    {
        private const string SIPSOFTPHONE_CONFIGNODE_NAME = "sipsoftphone";
        private const string SIPSOCKETS_CONFIGNODE_NAME = "sipsockets";
        private const string STUN_SERVER_KEY = "STUNServerHostname";
        private const string VIDEO_DEVICE_INDEX_KEY = "VideoDeviceIndex";
        private const int DEFAULT_VIDEO_DEVICE_INDEX = 0;

        private static ILog logger = AppState.logger;

        private static readonly XmlNode m_sipSoftPhoneConfigNode;
        public static readonly XmlNode SIPSocketsNode;
        public static readonly string STUNServerHostname;

        public static readonly string SIPUsername = "2001"  /*ConfigurationManager.AppSettings["SIPUsername"]*/;    // Get the SIP username from the config file.
        public static readonly string SIPPassword = "2001" /*ConfigurationManager.AppSettings["SIPPassword"]*/;    // Get the SIP password from the config file.
        public static readonly string SIPServer = "172.16.1.17"/*ConfigurationManager.AppSettings["SIPServer"]*/;        // Get the SIP server from the config file.
        public static readonly string SIPFromName ="2001" /*ConfigurationManager.AppSettings["SIPFromName"]*/;    // Get the SIP From display name from the config file.
        public static readonly string DnsServer ="" /*ConfigurationManager.AppSettings["DnsServer"]*/;        // Get the optional DNS server from the config file.
        private static readonly string pathAccount = @"Config\accounts.xml";
        public static IPAddress PublicIPAddress;
        static SIPSoftPhoneState()
        {
            try
            {
                //if (ConfigurationManager.GetSection(SIPSOFTPHONE_CONFIGNODE_NAME) != null)
                //{
                //    m_sipSoftPhoneConfigNode = (XmlNode)ConfigurationManager.GetSection(SIPSOFTPHONE_CONFIGNODE_NAME);
                //}

                //if (m_sipSoftPhoneConfigNode != null)
                //{
                //    SIPSocketsNode = m_sipSoftPhoneConfigNode.SelectSingleNode(SIPSOCKETS_CONFIGNODE_NAME);
                //}
                string applicationDirectory = Path.GetFullPath(pathAccount);
                if (File.Exists(pathAccount))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(pathAccount);
                    XmlNode root = doc.DocumentElement;
                    var xmlNodeList = root.SelectNodes("descendant::Account");
                    SIPUsername = xmlNodeList[0].SelectSingleNode("SIPUsername").InnerText;
                    SIPPassword = xmlNodeList[0].SelectSingleNode("SIPPassword").InnerText;
                    SIPFromName = xmlNodeList[0].SelectSingleNode("SIPFromName").InnerText;
                   
                }
               

                STUNServerHostname ="" /*ConfigurationManager.AppSettings[STUN_SERVER_KEY]*/;
            }
            catch (Exception excp)
            {
                logger.Error("Exception SIPSoftPhoneState. " + excp.Message);
                throw;
            }
        }
    }
}
