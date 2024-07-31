using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyManager : MonoBehaviour {
    [SerializeField]
    KeyCode m_appQuit = KeyCode.Escape;

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(m_appQuit))
        {
            Application.Quit();
        }
    }
}
