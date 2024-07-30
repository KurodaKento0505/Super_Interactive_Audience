using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PbiVr
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CommandBufferMaskApplyer))]
    public class DistortedDrawer : MonoBehaviour
    {
        #region Private Fields

        bool m_isInitialized = false;

        GameObject m_forLeftEye;
        GameObject m_forRightEye;

        [SerializeField]
        bool m_syncViewpointTransformWithObserver = true;

        [SerializeField]
        MeshFilter m_screenModel;
        [SerializeField]
        bool m_drawAsWireframe = false;
        GameObject m_screenGameObjLeft = null;
        GameObject m_screenGameObjRight = null;

        GameObject m_vProjBaseObjectLeft = null;
        GameObject m_vProjBaseObjectRight = null;
        [SerializeField]
        Material m_projectionMaterial;
        [SerializeField]
        Material m_screenBaseMaterial;
        List<Projector> m_vProjectorsLeft = new List<Projector>();
        List<Projector> m_vProjectorsRight = new List<Projector>();
        List<Projector> m_vProjectorsFloorLeft = new List<Projector>();
        List<Projector> m_vProjectorsFloorRight = new List<Projector>();
        List<Projector> m_vProjectorsCeilingLeft = new List<Projector>();
        List<Projector> m_vProjectorsCeilingRight = new List<Projector>();
        string m_layerNameLeft = "ScreenModelLeftEye";
        string m_layerNameRight = "ScreenModelRightEye";

        Observer m_observer=null;

        [SerializeField]
        GameObject m_projectorCamPrefab;
        [SerializeField]
        List<ProjectorParameter> m_projectorParams;
        List<Camera> m_projectorCamerasLeft;
        List<Camera> m_projectorCamerasRight;
        string m_fileName = "data.ini";
        string m_saveDir = "";
        string m_saveFullPath = "";

        CommandBufferMaskApplyer m_maskApplyer;


        #endregion

        #region Properties
        // Automatically sync transform with the object which has Observer component.
        public bool SyncViewpointTransformWithObserver
        {
            get { return m_syncViewpointTransformWithObserver; }
            set { m_syncViewpointTransformWithObserver = value; }
        }
        // Position of the user viewpoint inside the screen.
        public Vector3 ViewpointPositionInsideScreen
        {
            get { return m_vProjBaseObjectLeft.transform.position; }
            set
            {
                m_vProjBaseObjectLeft.transform.position = value;
                m_vProjBaseObjectRight.transform.position = value;
            }
        }
        // Rotation of the user viewpoint inside the screen.
        public Quaternion ViewpointRotationInsideScreen
        {
            get { return m_vProjBaseObjectLeft.transform.localRotation; }
            set
            {
                m_vProjBaseObjectLeft.transform.localRotation = value;
                m_vProjBaseObjectRight.transform.localRotation = value;
            }
        }
        #endregion

        void Reset()
        {
            #if UNITY_EDITOR
            AddLayer();
            #endif

            var prevs = FindObjectsOfType(typeof(DistortedDrawer));
            if (prevs.Length>1)
            {
                Debug.LogError("Initialized twice. Don't use DistortedDrawer twice in the scene hierarchy.");

                #if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("Warning!", "Initialized twice. Don't use DistortedDrawer twice in the scene hierarchy.", "OK");
                #endif
            }
            
        }

        // Use this for initialization
        void Start()
        {

        }

        void OnEnable()
        {
            init();
        }

        void OnDisable()
        {
            deinit();
        }

        [ContextMenu("AddLayer()")]
        void AddLayer()
        {
            #if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog("Notice", "DistortedDrawer component will add layers to project.", "OK", "Cancel"))
            {
                LayerMaskEx.CreateLayer(m_layerNameLeft);
                LayerMaskEx.CreateLayer(m_layerNameRight);
            }
            #endif
        }

        void init()
        {
            m_saveDir = Application.streamingAssetsPath;
            m_saveFullPath = m_saveDir + "/" + m_fileName;
            //if(m_projectorParams.Count ==0 ) LoadDataFrom(m_saveFullPath);

            //// change screen size
            //Screen.SetResolution(8400, 1050, false);
            
            m_forLeftEye = CreateEmpty("ForLeftEye", gameObject.transform);
            m_forRightEye = CreateEmpty("ForRightEye", gameObject.transform);

            // projector base obj
            m_vProjBaseObjectLeft = CreateEmpty("LeftVProjectorBase", m_forLeftEye.transform);
            m_vProjBaseObjectRight = CreateEmpty("RightVProjectorBase", m_forRightEye.transform); 

            // screen model
            m_screenGameObjLeft = CreateScreenModel(m_screenModel, m_layerNameLeft, m_drawAsWireframe);
            m_screenGameObjLeft.transform.parent = m_forLeftEye.transform;
            m_screenGameObjLeft.transform.localPosition = Vector3.zero;
            m_screenGameObjRight = CreateScreenModel(m_screenModel, m_layerNameRight, m_drawAsWireframe);
            m_screenGameObjRight.transform.parent = m_forRightEye.transform;
            m_screenGameObjRight.transform.localPosition = Vector3.zero;

            // setup cameras for real projector
            // If CameraPrefab is not seledted, apply default setting
            if (m_projectorCamPrefab == null) Debug.Log("DistortedDrawer: ProjectorCamPrefab is not selected. Create camera with default settings.");
            m_projectorCamerasLeft = SetupProjectorCameras("Left", m_layerNameLeft, m_projectorParams, m_screenGameObjLeft.transform, m_projectorCamPrefab);
            m_projectorCamerasRight = SetupProjectorCameras("Right", m_layerNameRight, m_projectorParams, m_screenGameObjRight.transform, m_projectorCamPrefab);
            foreach (var c in m_projectorCamerasLeft) c.stereoTargetEye = StereoTargetEyeMask.Left;
            foreach (var c in m_projectorCamerasRight) c.stereoTargetEye = StereoTargetEyeMask.Right;

            // setup blending masks
            m_maskApplyer = GetComponent<CommandBufferMaskApplyer>();
            for (int i = 0; i < m_projectorCamerasLeft.Count; ++i)
            {
                m_maskApplyer.AddMaskCommandBuffer(m_projectorCamerasLeft[i], m_projectorParams[i].MaskTexture);
            }
            for (int i = 0; i < m_projectorCamerasRight.Count; ++i)
            {
                m_maskApplyer.AddMaskCommandBuffer(m_projectorCamerasRight[i], m_projectorParams[i].MaskTexture);
            }

            // get camera info
            m_observer = FindObjectOfType<Observer>();
            if(m_observer == null)
            {
                Debug.LogError("There is no Observer component in this scene!");
                return;
            }

            //projectors setup
            Transform leftParent = m_vProjBaseObjectLeft.transform;
            Transform rightParent = m_vProjBaseObjectRight.transform;
            m_vProjectorsFloorLeft  = SetupVProjector(leftParent, "FloorLeft", m_layerNameLeft, m_projectionMaterial, m_observer.FloorCameraLeft);
            m_vProjectorsFloorRight = SetupVProjector(rightParent, "FloorRight", m_layerNameRight, m_projectionMaterial, m_observer.FloorCameraRight);
            m_vProjectorsCeilingLeft = SetupVProjector(leftParent, "CeilingLeft", m_layerNameLeft, m_projectionMaterial, m_observer.CeilingCameraLeft);
            m_vProjectorsCeilingRight = SetupVProjector(rightParent, "CeilingRight", m_layerNameRight, m_projectionMaterial, m_observer.CeilingCameraRight);
            m_vProjectorsLeft = SetupVProjector(leftParent, "Left", m_layerNameLeft, m_projectionMaterial, m_observer.PanoCamerasLeft);
            m_vProjectorsRight = SetupVProjector(rightParent, "Right", m_layerNameRight, m_projectionMaterial, m_observer.PanoCamerasRight);
            //Debug.Log(m_observer.PanoCamerasRight);

            // setup cam layer mask
            int screenLayer0 = 1 << LayerMask.NameToLayer(m_layerNameLeft);
            int screenLayer1 = 1 << LayerMask.NameToLayer(m_layerNameRight);
            int layerMask = ~(screenLayer0 | screenLayer1 | m_observer.IgnoreLayer.value);
            foreach (var c in m_observer.PanoCamerasLeft) c.cullingMask = layerMask;
            foreach(var c in m_observer.PanoCamerasRight) c.cullingMask = layerMask;
            foreach (var c in m_observer.FloorCameraLeft) c.cullingMask = layerMask;
            foreach (var c in m_observer.FloorCameraRight) c.cullingMask = layerMask;
            foreach (var c in m_observer.CeilingCameraLeft) c.cullingMask = layerMask;
            foreach (var c in m_observer.CeilingCameraRight) c.cullingMask = layerMask;

            // virtual projector base height
            // sync position
            if (m_syncViewpointTransformWithObserver)
            {
                m_vProjBaseObjectLeft.transform.localPosition = m_observer.transform.localPosition;
                m_vProjBaseObjectRight.transform.localPosition = m_observer.transform.localPosition;
            }
            //Vector3 currentPos = m_vProjBaseObjectLeft.transform.position;
            //m_vProjBaseObjectLeft.transform.position = new Vector3(currentPos.x, 1.6f, currentPos.z);
            //currentPos = m_vProjBaseObjectRight.transform.position;
            //m_vProjBaseObjectRight.transform.position = new Vector3(currentPos.x, 1.6f, currentPos.z);

            m_isInitialized = true;
        }

        GameObject CreateEmpty(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            return go;
        }

        GameObject CreateFromPrefab(string name, Transform parent, GameObject prefab)
        {
            GameObject go = Instantiate(prefab, parent);
            go.name = name;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            return go;
        }

        GameObject CreateScreenModel(MeshFilter meshFilter, string layerName, bool asWireframe = false)
        {
            // Model setup
            GameObject go = new GameObject(layerName);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            //go.hideFlags = HideFlags.NotEditable;
            go.layer = LayerMask.NameToLayer(layerName);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = meshFilter.sharedMesh;
            // build as Wireframe or not
            if (asWireframe) mf.sharedMesh.SetIndices(mf.sharedMesh.GetIndices(0), MeshTopology.Lines, 0);
            else mf.sharedMesh.SetIndices(mf.sharedMesh.GetIndices(0), MeshTopology.Triangles, 0);

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            mr.sharedMaterial = new Material(m_screenBaseMaterial);

            return go;
        }

        List<Projector> SetupVProjector(Transform parent, string namePrefix, string layerName, Material projectionMaterial, List<Camera> cams)
        //setup left 
        {
            List<Projector> projs = new List<Projector>();

            for (int i = 0; i < cams.Count; ++i)
            {
                // create virtual projector
                GameObject go = new GameObject(namePrefix + "VProjector (" + i.ToString() + ")");
                go.transform.parent = parent;
                go.transform.localPosition = cams[i].transform.localPosition;
                go.transform.localRotation = cams[i].transform.localRotation;
                go.layer = LayerMask.NameToLayer(layerName);
                //go.hideFlags = HideFlags.NotEditable;

                // setup virtual projector : copy from observer cams
                Projector proj = go.AddComponent<Projector>();
                proj.nearClipPlane = cams[i].nearClipPlane;
                proj.farClipPlane = cams[i].farClipPlane;
                proj.fieldOfView = cams[i].fieldOfView;
                proj.aspectRatio = cams[i].aspect;

                // layer mask
                int layerMask = 1 << LayerMask.NameToLayer(layerName);
                layerMask = ~layerMask;//反転
                proj.ignoreLayers = layerMask;

                // setup material
                Material mat = new Material(projectionMaterial);
                mat.SetTexture("_MainTex", cams[i].targetTexture);
                proj.material = mat;

                projs.Add(proj);
            }

            return projs;
        }


        void deinit()
        {
            m_isInitialized = false;

            //SaveTo(m_saveFullPath, true);

            // screen
            if (m_screenGameObjLeft != null) Destroy(m_screenGameObjLeft);
            if (m_screenGameObjRight != null) Destroy(m_screenGameObjRight);
            
            // virtual projector go
            DestroyVProjectorObjects(m_vProjectorsLeft);
            DestroyVProjectorObjects(m_vProjectorsRight);
            if (m_vProjBaseObjectLeft != null)Destroy(m_vProjBaseObjectLeft);
            if (m_vProjBaseObjectRight != null) Destroy(m_vProjBaseObjectRight);

            // blending masks
            m_maskApplyer.CleanupAll();

            // projector cams
            DestroyProjectorCameras(m_projectorCamerasLeft);
            DestroyProjectorCameras(m_projectorCamerasRight);

            // base
            if (m_forLeftEye != null) Destroy(m_forLeftEye);
            if (m_forRightEye != null) Destroy(m_forRightEye);
        }

        void DestroyVProjectorObjects(List<Projector> projs)
        {
            // destroy
            if (projs != null)
            {
                foreach (var proj in projs)
                {
                    if (proj != null)
                    {
                        Destroy(proj.gameObject);
                    }
                }
                projs.Clear();
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (!m_isInitialized) return;

            // sync position
            if (m_syncViewpointTransformWithObserver)
            {
                m_vProjBaseObjectLeft.transform.localPosition = m_observer.transform.localPosition;
                m_vProjBaseObjectRight.transform.localPosition = m_observer.transform.localPosition;
            }

            UpdateEyeDistance();

#if UNITY_EDITOR
            CopyCamerasToProjectorParams(m_projectorCamerasLeft, m_projectorParams);
#endif
        }

        void UpdateEyeDistance(){
            for (int i = 0; i < m_observer.PanoCamerasLeft.Count; ++i)      m_vProjectorsLeft[i].transform.localPosition = m_observer.PanoCamerasLeft[i].transform.localPosition;
            for (int i = 0; i < m_observer.PanoCamerasRight.Count; ++i)     m_vProjectorsRight[i].transform.localPosition = m_observer.PanoCamerasRight[i].transform.localPosition;
            for (int i = 0; i < m_observer.FloorCameraLeft.Count; ++i)      m_vProjectorsFloorLeft[i].transform.localPosition = m_observer.FloorCameraLeft[i].transform.localPosition;
            for (int i = 0; i < m_observer.FloorCameraRight.Count; ++i)     m_vProjectorsFloorRight[i].transform.localPosition = m_observer.FloorCameraRight[i].transform.localPosition;
            for (int i = 0; i < m_observer.CeilingCameraLeft.Count; ++i)    m_vProjectorsCeilingLeft[i].transform.localPosition = m_observer.CeilingCameraLeft[i].transform.localPosition;
            for (int i = 0; i < m_observer.CeilingCameraRight.Count; ++i)   m_vProjectorsCeilingRight[i].transform.localPosition = m_observer.CeilingCameraRight[i].transform.localPosition;
        }

        void SaveTo(string path, bool overwrite)
        {
            string data = JsonUtility.ToJson(new Serialization<ProjectorParameter>(m_projectorParams));
            if (File.Exists(path))
            {
                if (overwrite)
                {
                    File.Delete(path);
                }
                else
                {
                    Debug.LogWarning("File exist: " + path);
                    return;
                }
            }

            using (FileStream fs = File.Create(path))
            {
                fs.Close();
            }
            File.WriteAllText(path, data);
        }

        void LoadDataFrom(string path)
        {
            if (!File.Exists(path))
            {
                m_projectorParams = new List<ProjectorParameter>();
                return;
            }

            string data = "";
            using (var sr = File.OpenText(path))
            {
                data = sr.ReadToEnd();
                sr.Close();
            }

            try
            {
                m_projectorParams = JsonUtility.FromJson<Serialization<ProjectorParameter>>(data).ToList();
            }
            catch (System.Exception)
            {
                m_projectorParams = new List<ProjectorParameter>();
                return;
            }
        }

        void CopyCamerasToProjectorParams(List<Camera> cams, List<ProjectorParameter> projParams)
        {
            for (int i = 0; i < projParams.Count && i < cams.Count; ++i)
            {
                projParams[i].CopyValueeFrom(cams[i]);
            }

        }

        List<Camera> SetupProjectorCameras(string namePrefix, string layerName, List<ProjectorParameter> projParams, Transform parent, GameObject camPrefab)
        {
            List<Camera> cams = new List<Camera>();
            for (int i = 0; i < projParams.Count; ++i)
            {
                GameObject go;
                Camera cam;
                if (camPrefab == null)
                {
                    go = CreateEmpty(namePrefix + "ProjCam (" + i.ToString() + ")", parent);
                    cam = go.AddComponent<Camera>();
                }else{
                    go = CreateFromPrefab(namePrefix + "ProjCam (" + i.ToString() + ")", parent, camPrefab);
                    cam = go.GetComponent<Camera>();
                    if (cam == null) cam = go.AddComponent<Camera>();
                }
                projParams[i].CopyValuesTo(ref cam);

                cam.cullingMask = 1 << LayerMask.NameToLayer(layerName);

                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0, 0, 0, 0);

                // stereo settings
                cam.stereoSeparation = 0;
                cam.stereoConvergence = Mathf.Infinity;

                cams.Add(cam);
            }

            return cams;
        }

        void DestroyProjectorCameras(List<Camera> cams)
        {
            foreach (var c in cams) {
                if (c != null) Destroy(c.gameObject);
            }
        }

        public void DestroyProjectorCamerasRenge(int index, int count)
        {
            // TODO: detect "out of range" exeption
            for (int i = index; i < index + count; i++)
            {
                Destroy(m_projectorCamerasLeft[i].gameObject);
                Destroy(m_projectorCamerasRight[i].gameObject);
            }
            m_projectorCamerasLeft.RemoveRange(index, count);
            m_projectorCamerasRight.RemoveRange(index, count);
        }

        void OnValidate()
        {
            if (!m_isInitialized) return;

            if (m_projectorCamerasLeft != null && m_projectorParams != null)
            {
                for (int i = 0; i < m_projectorCamerasLeft.Count; ++i)
                {
                    Camera cam = m_projectorCamerasLeft[i];
                    m_projectorParams[i].CopyValuesTo(ref cam);
                }
            }
            if (m_projectorCamerasRight != null && m_projectorParams != null)
            {
                for (int i = 0; i < m_projectorCamerasRight.Count; ++i)
                {
                    Camera cam = m_projectorCamerasRight[i];
                    m_projectorParams[i].CopyValuesTo(ref cam);
                }
            }
        }

        [ContextMenu("Load Parameter")]
        void LoadParamFor_UnityEditor()
        {
            m_saveDir = Application.streamingAssetsPath;
            m_saveFullPath = m_saveDir + "/" + m_fileName;
            LoadDataFrom(m_saveFullPath);
        }
        [ContextMenu("Save Parameter")]
        void SaveParamTo_UnityEditor()
        {
            m_saveDir = Application.streamingAssetsPath;
            m_saveFullPath = m_saveDir + "/" + m_fileName;
            SaveTo(m_saveFullPath, true);
        }

    }
}