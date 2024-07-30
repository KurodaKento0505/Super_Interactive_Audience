using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lasp
{
    public class OptitrackRigidBodyApplyer : MonoBehaviour
    {
        OptitrackStreamingClient m_streamingClient;
        [SerializeField]
        System.Int32 m_rigidBodyId = 2;
        [SerializeField]
        bool m_freezeRotation = true;

        void Start()
        {
            if (m_streamingClient == null)
            {
                m_streamingClient = OptitrackStreamingClient.FindDefaultClient();

                if (m_streamingClient == null)
                {
                    Debug.LogError(GetType().FullName + ": Streaming client not set, and no " + typeof(OptitrackStreamingClient).FullName + " components found in scene; disabling this component.", this);
                    enabled = false;
                    return;
                }
            }
        }

        void OnEnable()
        {
            Application.onBeforeRender += OnBeforeRender;
        }


        void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
        }


        void OnBeforeRender()
        {
            UpdatePose();
        }

        void Update()
        {
            UpdatePose();
        }


        void UpdatePose()
        {
            OptitrackRigidBodyState rbState = m_streamingClient.GetLatestRigidBodyState(m_rigidBodyId);
            Debug.Log(rbState);
            if (rbState != null)
            {
                transform.localPosition = rbState.Pose.Position;
                if(!m_freezeRotation)transform.localRotation = rbState.Pose.Orientation;
            }
        }
    }
}