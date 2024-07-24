using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace lasp
{
    [DefaultExecutionOrder(-100)]//実行順を早めに
    [RequireComponent(typeof(OptitrackStreamingClient))]
    public class OptitrackIpSetup : MonoBehaviour
    {

        #region Fields
        static byte m_myIpMsb = 192;
        static IPAddress m_myIpTemplate = IPAddress.Parse("192.168.100.255");
        OptitrackStreamingClient m_sc = null;
        #endregion

        #region Properties
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
                        if (addrByte[0] == m_myIpMsb)// is large space cluster address?
                        {
                            localIpAddress = addr;
                        }
                    }

                    // cannot find appropriate IP adress 
                    if (localIpAddress == null)
                    {
                        //Debug.LogWarning("lasp::Utils: There is no valid LargeSpace-Cluster IP address.\nTemporarily, set cluster ID as " + ((int)ClusterId.Id00).ToString() + ".");
                        Debug.LogWarning("OptitrackIpSetup: OptitrackIpSetup: There is no valid IP address.");
                        localIpAddress = m_myIpTemplate;
                    }
                }

                return localIpAddress;
            }
        }
        #endregion

        // Use this for initialization
        void OnEnable()
        {
            m_sc = GetComponent<OptitrackStreamingClient>();
            m_sc.LocalAddress = LocalIpAddress.ToString();

            if(lasp.Constants.MyClusterId == Constants.ClusterId.NotCluster)
            {
                //m_sc.enabled = false;
                Debug.LogWarning("OptitrackIpSetup: This system is not LargeSpace-cluster. OptitrackStreamingClient component was disabled.");
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}