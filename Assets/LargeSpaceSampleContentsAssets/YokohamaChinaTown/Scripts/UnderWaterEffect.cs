using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PbiVr;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Renderer))]
public class UnderWaterEffect : MonoBehaviour {

	Observer m_observer=null;
	Transform m_tf=null;
	Renderer m_renderer=null;

	[SerializeField]
	[Range(0.0f, 1.0f)]
	float m_waterFogDensity = 0.5f;
	[SerializeField]
	Color m_waterFogColor = new Color (0.3f, 0.2f, 0.1f);

	bool IsUnderWater{
		get { 
			bool isLowerThanWaterPlane = m_tf.position.y > m_observer.gameObject.transform.position.y;
			return isLowerThanWaterPlane && m_renderer.enabled;
		}
	}

	// Use this for initialization
	void OnEnable () {
		m_renderer = GetComponent<Renderer> ();
		m_observer = FindObjectOfType<Observer> ();
		m_tf = gameObject.transform;
	}

	void OnDisable(){
		while (FogSettingStack.Count > 0) {
			FogSettingStack.Pop ();//restore
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (IsUnderWater) {
			if (FogSettingStack.Count==0) {
				FogSettingStack.Push ();//save

				RenderSettings.fog = true;
				RenderSettings.fogMode = FogMode.ExponentialSquared;
				RenderSettings.fogColor = m_waterFogColor;
				RenderSettings.fogDensity = m_waterFogDensity;
				RenderSettings.fogStartDistance = 1.0f;
				RenderSettings.fogEndDistance = 100.0f;
				}
		} else {
			if (FogSettingStack.Count > 0) {
				FogSettingStack.Pop ();//restore
			}
		}
	}
}
