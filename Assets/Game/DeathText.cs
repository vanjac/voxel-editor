using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathText : MonoBehaviour
{
    private UnityEngine.UI.Text text;
    private bool dead = false;

    void Start()
    {
        text = GetComponent<UnityEngine.UI.Text>();
    }

    void Update()
    {
        if (!dead)
        {
            Camera cam = Camera.current;
            if (cam != null && cam.tag == "DeathCamera")
            {
                dead = true;
                text.enabled = true;
            }
        }
    }
}