using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CarSpawner : NetworkBehaviour
{

    // 生成するプレハブ
    [SerializeField]
    GameObject m_waterPrefab;
    GameObject m_spawnedObject = null;

    [SerializeField]
    KeyCode m_spawnKey = KeyCode.D;

    void Update()
    {

        if (Input.GetKeyDown(m_spawnKey))
        {
            if (!NetworkManagerLS.instance.IsServer) return;

            if (FindObjectOfType<NetworkCarMover>() == null)
            {
                SpawnPrefab();
            }
            else
            {
                DestroyPrefab();
            }
        }
    }

    void SpawnPrefab()
    {
        if (m_spawnedObject != null)
        {
            Debug.Log("Already spawned.");
            return;
        }

        // spawn
        m_spawnedObject = Instantiate(m_waterPrefab);
        NetworkServer.Spawn(m_spawnedObject);
    }

    void DestroyPrefab()
    {
        NetworkServer.Destroy(m_spawnedObject);
        m_spawnedObject = null;
    }

}