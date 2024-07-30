using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PbiVr
{
    [DefaultExecutionOrder(-200)]//実行順を早めに
    public class Observer : MonoBehaviour
    {
        [SerializeField]
        GameObject m_cameraPrefab;
        [SerializeField]
        LayerMask m_ignoreLayer = new LayerMask();
        //List<RenderTexture> m_renderTextures = new List<RenderTexture>();
        [SerializeField]
        int m_sizeOfRenderTex = 1024;
        [SerializeField]
        int m_numOfRTexForHorisontalPanorama = 8;
        [SerializeField]
        float m_fieldOfViewYDegree = 120;
        [SerializeField]
        float m_near = 0.05f;
        [SerializeField]
        float m_far = 500.0f;
        [SerializeField]
        float m_screenDistanceM = 7.5f;

        [SerializeField]
        float m_eyeDistanceMm = 60;

        List<Camera> m_panoCamerasLeft = new List<Camera>();
        List<Camera> m_panoCamerasRight = new List<Camera>();
        List<Camera> m_floorCameraLeft = new List<Camera>();
        List<Camera> m_floorCameraRight = new List<Camera>();
        List<Camera> m_ceilingCameraLeft = new List<Camera>();
        List<Camera> m_ceilingCameraRight = new List<Camera>();

        enum Eye{Left, Right};

        bool m_isInitialized = false;

        public LayerMask IgnoreLayer
        {
            get { return m_ignoreLayer; }
            set { m_ignoreLayer = value; }
        }
        public bool IsInitialized
        {
            get { return m_isInitialized; }
        }

        public float EyeDistanceMm
        {
            get { return m_eyeDistanceMm; }
            set { 
                m_eyeDistanceMm = value;
                UpdateEyeDistance();
            }
        }

        public List<Camera> PanoCamerasLeft
        {
            get { return m_panoCamerasLeft; }
        }

        public List<Camera> PanoCamerasRight
        {
            get { return m_panoCamerasRight; }
        }

        public List<Camera> FloorCameraLeft
        {
            get { return m_floorCameraLeft; }
        }
        public List<Camera> FloorCameraRight
        {
            get { return m_floorCameraRight; }
        }

        public List<Camera> CeilingCameraLeft
        {
            get { return m_ceilingCameraLeft; }
        }
        public List<Camera> CeilingCameraRight
        {
            get { return m_ceilingCameraRight; }
        }

        void OnEnable()
        {
            init();
        }

        private void OnDisable()
        {
            deinit();
        }

        private void init()
        {
            if (m_isInitialized)
            {
                deinit();
            }

            // TODO: 以下のカメラセットアップの手順を整理したい。冗長かつ仕様変更に弱いので。
            {
                //床面天面の重なり量(degree)
                //眼間距離が600mmまで。それ以上は，床面を覆いきれない部分がでる。本来はスクリーンモデルまでの距離と，眼間距離によって床にできるテクスチャの隙間の大きさによる．
                float overlappedFovY = 24;

                // CameraPrefabがセットされていない場合はデフォルトセッティングで初期化する
                if(m_cameraPrefab==null) Debug.Log("Observer: CameraPrefab is not selected. Create camera with default settings.");

                m_panoCamerasLeft = CreatePanoramaCameraObjects("PanoCamLeft", -m_eyeDistanceMm / 2.0f, m_numOfRTexForHorisontalPanorama, m_sizeOfRenderTex, m_fieldOfViewYDegree, gameObject.transform, m_cameraPrefab);
                m_panoCamerasRight = CreatePanoramaCameraObjects("PanoCamRight", m_eyeDistanceMm / 2.0f, m_numOfRTexForHorisontalPanorama, m_sizeOfRenderTex, m_fieldOfViewYDegree, gameObject.transform, m_cameraPrefab);
                m_floorCameraLeft = CreateFloorCameraObjects("FloorCamLeft", -m_eyeDistanceMm / 2.0f, m_sizeOfRenderTex, 180 - m_fieldOfViewYDegree + overlappedFovY, gameObject.transform, m_cameraPrefab);
                m_floorCameraRight = CreateFloorCameraObjects("FloorCamRight", m_eyeDistanceMm / 2.0f, m_sizeOfRenderTex, 180 - m_fieldOfViewYDegree + overlappedFovY, gameObject.transform, m_cameraPrefab);
                m_ceilingCameraLeft = CreateFloorCameraObjects("CeilingCamLeft", -m_eyeDistanceMm / 2.0f, m_sizeOfRenderTex, 180 - m_fieldOfViewYDegree + overlappedFovY, gameObject.transform, m_cameraPrefab);
                m_ceilingCameraRight = CreateFloorCameraObjects("CeilingCamRight", m_eyeDistanceMm / 2.0f, m_sizeOfRenderTex, 180 - m_fieldOfViewYDegree + overlappedFovY, gameObject.transform, m_cameraPrefab);

                //pano left
                foreach (var cam in m_panoCamerasLeft) SetupFrustum(cam, Eye.Left, m_near, m_far, 0, m_screenDistanceM);
                //pano right
                foreach (var cam in m_panoCamerasRight) SetupFrustum(cam, Eye.Right, m_near, m_far, 0, m_screenDistanceM);
                //floor left
                foreach (var cam in m_floorCameraLeft) SetupFrustum(cam, Eye.Left, m_near, m_far, 0, m_screenDistanceM);
                //floor right
                foreach (var cam in m_floorCameraRight)  SetupFrustum(cam, Eye.Right, m_near, m_far, 0, m_screenDistanceM);
                //ceiling left
                foreach (var cam in m_ceilingCameraLeft)
                {
                    cam.gameObject.transform.Rotate(new Vector3(180, 0, 0));// look up
                    SetupFrustum(cam, Eye.Left, m_near, m_far, 0, m_screenDistanceM);
                }
                //ceiling right
                foreach (var cam in m_ceilingCameraRight)
                {
                    cam.gameObject.transform.Rotate(new Vector3(180, 0, 0));// look up
                    SetupFrustum(cam, Eye.Right, m_near, m_far, 0, m_screenDistanceM);
                }
            }
            m_isInitialized = true;
        }

        void SetupFrustum(Camera cam, Eye eye, float near, float far, float eyeDistanceMm,float screenDistanceM)
        {
            // setup frustum
            cam.nearClipPlane = near;
            cam.farClipPlane = far;
            float halfNearPlaneHeight = near * Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2);
            float halfNearPlaneWidth = halfNearPlaneHeight * cam.aspect;
            float halfEyeDistanceM = eyeDistanceMm / 1000.0f / 2.0f;
            float shift = halfEyeDistanceM * near / screenDistanceM;

            if (eye == Eye.Right) shift = -shift;

            cam.SetStereoProjectionMatrix(
                eye==Eye.Left ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right,
                Matrix4x4.Frustum(
                    -halfNearPlaneWidth + shift,
                    halfNearPlaneWidth + shift,
                    -halfNearPlaneHeight,
                    halfNearPlaneHeight,
                    near, far)
            );
        }

        List<Camera> CreatePanoramaCameraObjects(string baseName, float horizontalShiftMm, int numOfRTexForHorisontalPanorama, int sizeOfRenderTex, float fieldOfViewYDegree, Transform parent, GameObject cameraPrefab)
        {
            List<Camera> createdCameras = new List<Camera>();

            // create panorama cam objects
            for (int i = 0; i < numOfRTexForHorisontalPanorama; ++i)
            {
                GameObject go;
                Camera cam;

                if (cameraPrefab == null)
                {
                    go = new GameObject();
                    cam = go.AddComponent<Camera>();
                }
                else
                {
                    go = Instantiate(cameraPrefab);
                    cam = go.GetComponent<Camera>();
                }

                go.name = baseName + " (" + createdCameras.Count.ToString() + ")";
                go.transform.parent = parent;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                //go.hideFlags = HideFlags.NotEditable;

                // setup GO pos & rotation
                float degreeInterval = 360.0f / numOfRTexForHorisontalPanorama;
                go.transform.localRotation = Quaternion.Euler(0, degreeInterval * i, 0);
                go.transform.Translate(horizontalShiftMm / 1000.0f, 0, 0);

                // camera setup
                // target texture
                RenderTexture rt = new RenderTexture(sizeOfRenderTex, sizeOfRenderTex, 16);
                cam.targetTexture = rt;
                // render to single eye
                cam.stereoTargetEye = StereoTargetEyeMask.Left;
                cam.stereoSeparation = 0;
                cam.stereoConvergence = Mathf.Infinity;
                // setup aspect
                cam.fieldOfView = fieldOfViewYDegree;
                float halfAngleRadV = fieldOfViewYDegree / 2.0f * Mathf.Deg2Rad;
                float halfAngleRadH = 360.0f / numOfRTexForHorisontalPanorama / 2.0f * Mathf.Deg2Rad;
                cam.aspect = Mathf.Tan(halfAngleRadH) / Mathf.Tan(halfAngleRadV);

                createdCameras.Add(cam);
            }

            return createdCameras;
        }

        List<Camera> CreateFloorCameraObjects(string baseName, float horizontalShiftMm, int sizeOfRenderTex, float fieldOfViewYDegree, Transform parent, GameObject cameraPrefab)
        {
            List<Camera> createdCameras = new List<Camera>();

            GameObject go;
            Camera cam;

            if (cameraPrefab == null)
            {
                go = new GameObject();
                cam = go.AddComponent<Camera>();
            }
            else
            {
                go = Instantiate(cameraPrefab);
                cam = go.GetComponent<Camera>();
            }

                go.name = baseName + " (" + createdCameras.Count.ToString() + ")";
                go.transform.parent = parent;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                //go.hideFlags = HideFlags.NotEditable;

                // setup GO pos & rotation
                go.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));//鉛直下向き
                go.transform.Translate(horizontalShiftMm / 1000.0f, 0, 0);

                // camera setup
                // target texture
                RenderTexture rt = new RenderTexture(sizeOfRenderTex, sizeOfRenderTex, 16);
                cam.targetTexture = rt;
                // render to single eye
                cam.stereoTargetEye = StereoTargetEyeMask.Left;
                cam.stereoSeparation = 0;
                cam.stereoConvergence = Mathf.Infinity;
                // setup aspect
                cam.fieldOfView = fieldOfViewYDegree;
                cam.aspect = 1;

                createdCameras.Add(cam);

            return createdCameras;
        }

        void DestroyCameraList(List<Camera> cams)
        {
            // destroy panorama cams
            foreach (var cam in cams)
            {
                if (cam != null)
                {
                    cam.enabled = false;
                    if(cam.targetTexture!=null) Destroy(cam.targetTexture);
                    Destroy(cam.gameObject);
                }
            }
            cams.Clear();
        }

        private void deinit()
        {
            DestroyCameraList(m_panoCamerasLeft);
            DestroyCameraList(m_panoCamerasRight);
            DestroyCameraList(m_floorCameraLeft);
            DestroyCameraList(m_floorCameraRight);
            DestroyCameraList(m_ceilingCameraLeft);
            DestroyCameraList(m_ceilingCameraRight);

            m_isInitialized = false;
        }

        void UpdateEyeDistance()
        {
            foreach (var cam in m_panoCamerasLeft)
            {
                cam.gameObject.transform.localPosition = Vector3.zero;
                cam.gameObject.transform.Translate(-m_eyeDistanceMm / 2f / 1000.0f, 0, 0);
            }
            foreach (var cam in m_panoCamerasRight)
            {
                cam.gameObject.transform.localPosition = Vector3.zero;
                cam.gameObject.transform.Translate(m_eyeDistanceMm / 2f / 1000.0f, 0, 0);
            }
            foreach (var cam in m_floorCameraLeft)
            {
                cam.gameObject.transform.localPosition = Vector3.zero;
                cam.gameObject.transform.Translate(-m_eyeDistanceMm / 2f / 1000.0f, 0, 0);
            }
            foreach (var cam in m_floorCameraRight)
            {
                cam.gameObject.transform.localPosition = Vector3.zero;
                cam.gameObject.transform.Translate(m_eyeDistanceMm / 2f / 1000.0f, 0, 0);
            }
            foreach (var cam in m_ceilingCameraLeft)
            {
                cam.gameObject.transform.localPosition = Vector3.zero;
                cam.gameObject.transform.Translate(-m_eyeDistanceMm / 2f / 1000.0f, 0, 0);
            }
            foreach (var cam in m_ceilingCameraRight)
            {
                cam.gameObject.transform.localPosition = Vector3.zero;
                cam.gameObject.transform.Translate(m_eyeDistanceMm / 2f / 1000.0f, 0, 0);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void Reset()
        {
            var prevs = FindObjectsOfType(typeof(Observer));
            if (prevs.Length>1)
            {
                Debug.LogError("Initialized twice. Don't use Observer twice in the scene hierarchy.");

                #if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("Warning!", "Initialized twice. Don't use Observer twice in the scene hierarchy.", "OK");
                #endif
            }
        }

        void OnValidate()
        {
            if (!m_isInitialized) return;

            UpdateEyeDistance();
        }
    }
}