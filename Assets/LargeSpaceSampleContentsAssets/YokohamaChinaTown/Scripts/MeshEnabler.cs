using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MeshEnabler : MonoBehaviour {

	[SerializeField]
	KeyCode m_toggleKey = KeyCode.Space;
	Renderer m_renderer=null;

	void Start()
	{
		m_renderer = GetComponent<Renderer> ();
	}

	void Update () 
	{
		if (Input.GetKeyDown (m_toggleKey)) {
			if (m_renderer != null) {
				m_renderer.enabled = !m_renderer.enabled;
			}
		}
	}
}
