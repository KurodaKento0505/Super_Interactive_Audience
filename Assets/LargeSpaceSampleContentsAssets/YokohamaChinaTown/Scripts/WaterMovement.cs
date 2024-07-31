using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
public class WaterMovement : NetworkBehaviour
{

    //[SerializeField]
    Transform m_target;
    [SerializeField]
    float m_maxHeight = 1.5f;
    [SerializeField]
    float m_minHeight = 0.2f;
    [SerializeField]
    float m_cycleTimeSec = 10.0f;

    [SerializeField] float m_syncLerpRate = 4f;

    Vector3 m_receivedPosition;
    Quaternion m_receivedRotation;

    float m_enabledTime = 0f;
    [SerializeField]
    float m_startThetaDeg = -90.0f;

    // Use this for initialization
    void OnEnable () 
    {
        m_target = gameObject.transform;

        m_receivedPosition = m_target.position;
        m_receivedRotation = m_target.rotation;

        m_enabledTime = Time.unscaledTime;
    }

    void OnDisable()
    {
        m_enabledTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManagerLS.instance.IsServer)
        {
            if (!m_target) return;

            float waveWidth = m_maxHeight - m_minHeight;
            float offset = m_minHeight + (m_maxHeight - m_minHeight) * 0.5f;
            float theta = (m_startThetaDeg * Mathf.Deg2Rad) + (Time.unscaledTime - m_enabledTime) / m_cycleTimeSec * Mathf.PI;

            float waterHeight = offset + waveWidth * Mathf.Sin(theta) * 0.5f;

            Vector3 newPosition = new Vector3(m_target.position.x, waterHeight, m_target.position.z);

            // send to all machines
            CmdSyncTransform(newPosition, m_target.rotation);
        }

        // apply received value
        InterpolateTransform(m_target, m_receivedPosition, m_receivedRotation);
    }

    void InterpolateTransform(Transform target, Vector3 position, Quaternion rotation)
    {
        Vector3 pos = Vector3.Lerp(target.position, position, m_syncLerpRate * Time.deltaTime);
        Quaternion rot = Quaternion.Slerp(target.rotation, rotation, m_syncLerpRate * Time.deltaTime);
        target.SetPositionAndRotation(pos, rot);
    }

    // on Server
    [Command]
    void CmdSyncTransform(Vector3 position, Quaternion rotation)
    {
        foreach (var conn in NetworkServer.connections)
        {
            // ignore invalid connection
            if (conn == null || !conn.isReady)
                continue;

            TargetSyncTransform(conn, position, rotation);
        }
    }

    // on Target
    [TargetRpc]
    void TargetSyncTransform(NetworkConnection target, Vector3 position, Quaternion rotation)
    {
        m_receivedPosition = position;
        m_receivedRotation = rotation;
    }
}
