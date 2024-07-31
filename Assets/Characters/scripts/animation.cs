using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animation : MonoBehaviour
{
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetInteger("motion", 0);//reset
        if (Input.GetKeyDown("a"))
        {
            anim.SetInteger("motion", 1);
        }
        if (Input.GetKeyDown("s"))
        {
            anim.SetInteger("motion", 2);
        }
        if (Input.GetKeyDown("d"))
        {
            anim.SetInteger("motion", 3);
        }
        if (Input.GetKeyDown("f"))
        {
            anim.SetInteger("motion", 4);
        }

    }
}
