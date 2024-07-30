using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using lasp;

[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(NetworkDiscoveryLS))]
public class NetworkManagerLS : MonoBehaviour
{

    [SerializeField]
    Constants.ClusterId m_server = Constants.ClusterId.Id00;
    [SerializeField]
    int m_maxClientNumber = 1;
    [SerializeField]
    List<NetworkIdentity> m_serverControlObjects = new List<NetworkIdentity>();

    NetworkManager m_netManager;
    NetworkDiscoveryLS m_netDiscovery;

    [SerializeField]
    bool m_forceServer = false;
    [SerializeField]
    bool m_forceClient = false;

    #region Properties

    public bool IsServer{
        get { return 
            m_server == Constants.MyClusterId 
            || m_forceServer; 
        }
    }
    public bool IsClient{
        get { return
            (Constants.MyClusterId != m_server && Constants.MyClusterId != Constants.ClusterId.NotCluster)
            || m_forceClient;
        }
    }

    #endregion

    void OnEnable () 
    {
        //initialize
        m_netManager = GetComponent<NetworkManager>();
        m_netDiscovery = GetComponent<NetworkDiscoveryLS>();
        m_netDiscovery.OnReceivedBroadcastActions += OnReceivedBroadcast;

        if(IsServer){
            m_netManager.StartHost();

            m_netDiscovery.Initialize();
            m_netDiscovery.StartAsServer();
        }
        if(IsClient){
            m_netDiscovery.Initialize();
            m_netDiscovery.StartAsClient();
        }

        //get objects authority if this instance runs as server
        bool hasAuthority = IsServer;
        foreach (var id in m_serverControlObjects)
        {
            id.localPlayerAuthority = hasAuthority;
        }
    }


    void OnDisable()
    {
        m_netDiscovery.OnReceivedBroadcastActions -= OnReceivedBroadcast;
        m_netManager.StopHost();
    }


    // Update is called once per frame
    void Update () {

        if(IsServer){
            // if all clients are connected, stop broadcasting
            if (m_netManager.numPlayers > m_maxClientNumber){
                if(m_netDiscovery.running) m_netDiscovery.StopBroadcast();
            }
            else{
                if(!m_netDiscovery.running)m_netDiscovery.StartAsServer();
            }
        }

	}

    // for client
    void OnReceivedBroadcast(string fromAddress, string data)
    {
        if (IsServer) return;

        m_netManager.networkAddress = fromAddress;
        if (m_netManager.IsClientConnected()) return;

        Debug.LogFormat("OnReceivedBroadcast; fromAddress: {0}, data: {1}", fromAddress, data);
        m_netManager.StartClient();
    }


    static NetworkManagerLS _instance;

    public static NetworkManagerLS instance
    {
        get
        {
            if (_instance == null)
            {
                var previous = FindObjectOfType(typeof(NetworkManagerLS));
                if (previous)
                {
                    Debug.LogWarning("NetworkManagerLS is initialized twice.");
                    _instance = (NetworkManagerLS)previous;
                }
                else
                {
                    var go = new GameObject("__NetworkManagerLS");
                    _instance = go.AddComponent<NetworkManagerLS>();
                    DontDestroyOnLoad(go);
                    go.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            return _instance;
        }
    }
}
