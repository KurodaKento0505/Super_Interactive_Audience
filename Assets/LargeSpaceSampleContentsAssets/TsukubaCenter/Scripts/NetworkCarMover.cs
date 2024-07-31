using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkCarMover : NetworkBehaviour {

    //[SerializeField]
    Transform m_target;

    [SerializeField]
    Vector3 m_start = new Vector3();
    [SerializeField]
    Vector3 m_goal = new Vector3();
    [SerializeField]
    float m_goalAreaRadiusM = 0.5f;
    [SerializeField]
    float m_speedKmPerHour = 30.0f;

    Rigidbody m_rigidbody;

    [SerializeField] float m_syncLerpRate = 4f;

    Vector3 m_receivedPosition;
    Quaternion m_receivedRotation;
    Vector3 m_receivedVelocity;
    Vector3 m_receivedAngularVelocity;
    bool m_receivedResetPosition;

    void OnEnable()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_target = gameObject.transform;
    }
    void OnDisable()
    {
        // set velocity to Zero when disabled
        m_rigidbody.velocity = Vector3.zero;
    }

    void Update()
    {
        if (NetworkManagerLS.instance.IsServer)
        {
            if (!m_target) return;

            // calculate velocity meter per second
            float mPerSec = m_speedKmPerHour * 1000 / 3600f;
            Vector3 direction = Vector3.Normalize(m_goal - m_target.position);
            Vector3 velocity = direction * mPerSec;

            // set velocity
            // m_rigidbody.velocity = velocity;//not recommended officially ( https://docs.unity3d.com/ja/2017.4/ScriptReference/Rigidbody-velocity.html )
            CmdSyncVelocity(velocity, m_rigidbody.angularVelocity);

            // reset position to m_start if this object goals
            if (Vector3.Distance(m_goal, m_target.position) < m_goalAreaRadiusM)
            {
                // send to all machines
                CmdResetTransform(m_start, m_target.rotation);
            }
        }

        // apply
        m_rigidbody.velocity = m_receivedVelocity;
        m_rigidbody.angularVelocity = m_receivedAngularVelocity;
        // apply received transform
        if (m_receivedResetPosition) {
            //InterpolateTransform(m_target, m_receivedPosition, m_receivedRotation);
            m_target.position = m_receivedPosition;
            m_target.rotation = m_receivedRotation;
            m_receivedResetPosition = false;
        }
    }

    void InterpolateTransform(Transform target, Vector3 position, Quaternion rotation)
    {
        Vector3 pos = Vector3.Lerp(target.position, position, m_syncLerpRate * Time.deltaTime);
        Quaternion rot = Quaternion.Slerp(target.rotation, rotation, m_syncLerpRate * Time.deltaTime);
        target.SetPositionAndRotation(pos, rot);
    }


    // on Server
    [Command]
    void CmdResetTransform(Vector3 position, Quaternion rotation)
    {
        foreach (var conn in NetworkServer.connections)
        {
            // ignore invalid connection
            if (conn == null || !conn.isReady)
                continue;

            TargetResetTransform(conn, position, rotation);
        }
    }
    // on Server
    [Command]
    void CmdSyncVelocity(Vector3 velocity, Vector3 angularVelocity)
    {
        foreach (var conn in NetworkServer.connections)
        {
            // ignore invalid connection
            if (conn == null || !conn.isReady)
                continue;

            TargetSyncVelocity(conn, velocity, angularVelocity);
        }
    }

    // on Target
    [TargetRpc]
    void TargetResetTransform(NetworkConnection target, Vector3 position, Quaternion rotation)
    {
        m_receivedPosition = position;
        m_receivedRotation = rotation;
        m_receivedResetPosition = true;
    }
    // on Target
    [TargetRpc]
    void TargetSyncVelocity(NetworkConnection target, Vector3 velosity, Vector3 angularVelocity)
    {
        m_receivedVelocity = velosity;
        m_receivedAngularVelocity = angularVelocity;
    }
}
