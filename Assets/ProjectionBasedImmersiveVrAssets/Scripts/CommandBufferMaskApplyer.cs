using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PbiVr
{
    public class CommandBufferMaskApplyer : MonoBehaviour
    {
        [SerializeField]
        Material m_maskMaterial;
        [SerializeField]
        string m_maskTextureName = "_MaskTex";

        private Dictionary<Camera, CommandBuffer> m_cameras = new Dictionary<Camera, CommandBuffer>();
        CameraEvent m_commandTiming = CameraEvent.AfterImageEffects;

        // 全ての CommandBUffer をクリア
        public void CleanupAll()
        {
            List<Camera> keyList = new List<Camera>(m_cameras.Keys);
            foreach (var key in keyList)
            {
                if (key != null)
                {
                    key.RemoveCommandBuffer(m_commandTiming, m_cameras[key]); ;
                    m_cameras.Remove(key);
                }
            }
            m_cameras.Clear();
        }

        public void AddMaskCommandBuffer(Camera cam, Texture maskTex)
        {
            CommandBuffer buf = null;
            if (m_cameras.ContainsKey(cam))
            {
                return;
            }
            else
            {
                buf = new CommandBuffer();
                buf.name = "BlendingMask";
                m_cameras[cam] = buf;
            }

            if (m_maskMaterial == null) { return; }
            if (cam == null) { return; }
            if (maskTex == null) { return; }

            var mat = new Material(m_maskMaterial); // Shaderからマテリアルを作成する
            mat.SetTexture(m_maskTextureName, maskTex);//マテリアルにテクスチャを転送

            // ポストエフェクトのCommandBufferをセット
            buf.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, mat);
            
            // カメラへCommandBufferを追加
            cam.AddCommandBuffer(m_commandTiming, buf);
        }

        public void DeleteMaskCommandBuffer(Camera cam)
        {
            if (cam == null) { return; }

            List<Camera> keyList = new List<Camera>(m_cameras.Keys);
            foreach (var key in keyList)
            {
                if (key && key == cam)
                {
                    key.RemoveCommandBuffer(m_commandTiming, m_cameras[key]); ;
                    m_cameras.Remove(key);
                }
            }
        }
    }
}