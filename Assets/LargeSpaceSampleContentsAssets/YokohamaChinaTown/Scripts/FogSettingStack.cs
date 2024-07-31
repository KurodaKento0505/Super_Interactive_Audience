using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FogSettings {
	[SerializeField]
	bool m_fog = true;
	[SerializeField]
	FogMode m_fogMode = FogMode.ExponentialSquared;
	[SerializeField]
	Color m_fogColor = new Color (0.6f, 0.2f, 0.1f);
	[SerializeField]
	float m_fogDensity = 0.3f;
	[SerializeField]
	float m_fogStartDistance = 1.0f;
	[SerializeField]
	float m_fogEndDistance = 100.0f;

	public bool Fog {
		get { return m_fog;}
		set {m_fog = value;}
	}
	public FogMode FogMode {
		get {return m_fogMode;}
		set {m_fogMode = value;}
	}
	public Color FogColor {
		get {return m_fogColor;}
		set {m_fogColor = value;}
	}
	public float FogDensity {
		get {return m_fogDensity;}
		set {m_fogDensity = value;}
	}
	public float FogStartDistance {
		get {return m_fogStartDistance;}
		set {m_fogStartDistance = value;}
	}
	public float FogEndDistance {
		get {return m_fogEndDistance;}
		set {m_fogEndDistance = value;}
	}

	public FogSettings(){
		m_fog = RenderSettings.fog;
		m_fogMode = RenderSettings.fogMode;
		m_fogColor = RenderSettings.fogColor;
		m_fogDensity = RenderSettings.fogDensity;
		m_fogStartDistance = RenderSettings.fogStartDistance;
		m_fogEndDistance = RenderSettings.fogEndDistance;
	}
}

public static class FogSettingStack{
	static Stack<FogSettings> m_fogStack = new Stack<FogSettings>();

	public static int Count{
		get{ return m_fogStack.Count;}
	}

	public static void Push(){
		m_fogStack.Push (new FogSettings ());
	}
	public static void Pop(){
		if (Count == 0) return;

		var settings = m_fogStack.Pop ();
		RenderSettings.fog = settings.Fog;
		RenderSettings.fogMode = settings.FogMode;
		RenderSettings.fogColor = settings.FogColor;
		RenderSettings.fogDensity = settings.FogDensity;
		RenderSettings.fogStartDistance = settings.FogStartDistance;
		RenderSettings.fogEndDistance = settings.FogEndDistance;
	}
}
