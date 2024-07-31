using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleObjectMover : MonoBehaviour {

    [SerializeField]
    Vector3 m_start = new Vector3();
    [SerializeField]
    Vector3 m_goal = new Vector3();
    [SerializeField]
    float m_goalAreaRadiusM = 0.5f;
    [SerializeField]
    float m_speedKmPerHour = 30.0f;

    Rigidbody m_rigidbody;

    void OnEnable () 
    {
        m_rigidbody = GetComponent<Rigidbody>();
	}
    void OnDisable()
    {
        // set velocity to Zero when disabled
        m_rigidbody.velocity = Vector3.zero;
    }

    void Update () {

        // calculate velocity meter per second
        float mPerSec = m_speedKmPerHour * 1000 / 3600f;
        Vector3 direction = Vector3.Normalize(m_goal - transform.position);
        Vector3 velocity = direction * mPerSec;

        // set velocity
        m_rigidbody.velocity = velocity;//not recommended officially ( https://docs.unity3d.com/ja/2017.4/ScriptReference/Rigidbody-velocity.html )

        // reset position to m_start if this object goals
        if (Vector3.Distance(m_goal, transform.position) < m_goalAreaRadiusM){
            transform.position = m_start;
        }
	}
}
