using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

namespace lasp
{
    public static class Constants
    {
        public enum ClusterId
        {
            Id00 = 11,
            Id01 = 12,
            NotCluster = 99
        }

        #region Fields
        private static byte m_clusterIpMsb = 10;
        private static IPAddress m_clusterIpTemplate = IPAddress.Parse("10.0.2.255");
        #endregion


        #region Properties

        public static byte ClusterIpMsb
        {
            get { return m_clusterIpMsb; }
        }

        public static IPAddress ClusterIpTemplate
        {
            get { return m_clusterIpTemplate; }
        }

        static IPAddress LocalIpAddress
        {
            get
            {
                IPAddress localIpAddress = null;

                {
                    string hostName = Dns.GetHostName();
                    IPAddress[] addresses = Dns.GetHostAddresses(hostName);
                    foreach (IPAddress addr in addresses)
                    {
                        byte[] addrByte = addr.GetAddressBytes();
                        if (addrByte[0] == ClusterIpMsb)// is large space cluster address?
                        {
                            localIpAddress = addr;
                        }
                    }

                    // cannot find appropriate IP adress 
                    if (localIpAddress == null)
                    {
                        //Debug.LogWarning("lasp::Utils: There is no valid LargeSpace-Cluster IP address.\nTemporarily, set cluster ID as \"NotCluster\".");
                        localIpAddress = ClusterIpTemplate;
                    }
                }

                return localIpAddress;
            }
        }

        public static ClusterId MyClusterId
        {
            get
            {
                byte ipLsb = LocalIpAddress.GetAddressBytes()[3];
                ClusterId thisClusterId = ClusterId.NotCluster;
                foreach (ClusterId id in System.Enum.GetValues(typeof(ClusterId)))
                {
                    if (((int)ipLsb) == ((int)id))
                    {
                        thisClusterId = id;
                    }
                }

                return thisClusterId;
            }
        }

        #endregion
    }
}