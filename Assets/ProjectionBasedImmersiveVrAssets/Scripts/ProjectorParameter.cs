using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PbiVr
{
    [System.Serializable]
    public class ProjectorParameter
    {
        #region Fields
        [SerializeField]
        Vector3 m_localPosition = Vector3.zero;
        [SerializeField]
        Quaternion m_localRotation = Quaternion.identity;

        [SerializeField]
        float m_near = 0.01f;
        [SerializeField]
        float m_far = 50.0f;
        [SerializeField]
        float m_width = 1.920f;
        [SerializeField]
        float m_height = 1.080f;
        [SerializeField]
        float m_distanceFromScreen = 1.0f;
        [SerializeField]
        float m_imageOffsetYRate = 0;

        [SerializeField]
        Rect m_viewport = default(Rect);

        [SerializeField]
        Texture m_maskTexture = default(Texture);
        #endregion

        #region Properties
        public Vector3 LocalPosition
        {
            get { return m_localPosition; }
            set { m_localPosition = value; }
        }

        public Quaternion LocalRotation
        {
            get { return m_localRotation; }
            set { m_localRotation = value; }
        }

        public float Near
        {
            get { return m_near; }
            set { m_near = value; }
        }

        public float Far
        {
            get { return m_far; }
            set { m_far = value; }
        }

        public float Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        public float Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public float ImageOffsetYRate
        {
            get { return m_imageOffsetYRate; }
            set { m_imageOffsetYRate = value; }
        }

        public Rect Viewprot
        {
            get { return m_viewport; }
            set { m_viewport = value; }
        }

        public Texture MaskTexture
        {
            get { return m_maskTexture; }
            set { m_maskTexture = value; }
        }

        #endregion

        public Matrix4x4 GetProjectionMatrix()
        {
            float left  = -(m_near * m_width / m_distanceFromScreen / 2.0f);
            float right =  (m_near * m_width / m_distanceFromScreen / 2.0f);

            float nearHeight = m_height * m_near / m_distanceFromScreen;
            float bottom= -nearHeight / 2.0f + (nearHeight / 2.0f) * m_imageOffsetYRate;
            float top   =  nearHeight / 2.0f + (nearHeight / 2.0f) * m_imageOffsetYRate;

            return Matrix4x4.Frustum( left, right, bottom, top, m_near, m_far);
        }

        public void CopyValuesTo(ref Camera cam)
        {
            cam.nearClipPlane = Near;
            cam.farClipPlane = Far;
            cam.transform.localPosition = LocalPosition;
            cam.transform.localRotation = LocalRotation;
            cam.rect = Viewprot;
            float nearHeight = Height * Near / m_distanceFromScreen;
            cam.fieldOfView = Mathf.Atan2(nearHeight * 0.5f, Near) * Mathf.Rad2Deg * 2f;
            cam.projectionMatrix = GetProjectionMatrix();
            
        }

        public void CopyValueeFrom(Camera cam)
        {
            m_near =  cam.nearClipPlane;
            m_far = cam.farClipPlane;
            m_localPosition = cam.transform.localPosition;
            m_localRotation = cam.transform.localRotation;
            m_viewport = cam.rect;
        }

    }
}