using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationSetup : MonoBehaviour {

    [SerializeField]
    bool m_forceFullScreen = true;

    [SerializeField]
    bool m_cursorVisible = false;

	// Use this for initialization
	void OnEnable () {
        WindowTopmost.MakeTopmost();
        Screen.fullScreen = m_forceFullScreen;
        Cursor.visible = m_cursorVisible;
	}
	
	// Update is called once per frame
	void OnDisable () {
		
	}
}
