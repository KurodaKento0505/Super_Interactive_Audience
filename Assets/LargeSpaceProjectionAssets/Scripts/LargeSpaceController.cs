using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PbiVr;

namespace lasp
{
    [RequireComponent(typeof(DistortedDrawer))]
    public class LargeSpaceController : MonoBehaviour
    {
        Constants.ClusterId m_myClusterId = Constants.MyClusterId;
        DistortedDrawer m_distortedDrawer = null;

        // Use this for initialization
        void Start()
        {
            m_distortedDrawer = GetComponent<DistortedDrawer>();

            // destroy disuse Cams
            int deleteCamIndex = 0;
            int deleteCamCount = 0;
            switch (m_myClusterId)
            {
                case Constants.ClusterId.Id00:
                    deleteCamIndex = 6;
                    deleteCamCount = 6;
                    break;
                case Constants.ClusterId.Id01:
                    deleteCamIndex = 0;
                    deleteCamCount = 6;
                    break;
                case Constants.ClusterId.NotCluster:
                    deleteCamIndex = 6;
                    deleteCamCount = 6;
                    break;
            }
            m_distortedDrawer.DestroyProjectorCamerasRenge(deleteCamIndex, deleteCamCount);
        }
    }
}