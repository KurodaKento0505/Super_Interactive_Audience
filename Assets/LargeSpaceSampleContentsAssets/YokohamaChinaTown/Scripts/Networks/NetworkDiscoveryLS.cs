using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkDiscoveryLS : NetworkDiscovery {
    System.Action<string, string> m_onReceivedBroadcastActions;

    public Action<string, string> OnReceivedBroadcastActions
    {
        get { return m_onReceivedBroadcastActions; }
        set { m_onReceivedBroadcastActions = value; }
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        if(m_onReceivedBroadcastActions!=null)m_onReceivedBroadcastActions(fromAddress, data);
    }
}
