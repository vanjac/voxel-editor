using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathText : MonoBehaviour
{
    private UnityEngine.UI.Text text;
    private bool alive = false;
    private bool dead = false;

    void Start()
    {
        text = GetComponent<UnityEngine.UI.Text>();
    }

    void Update()
    {
        Camera cam = Camera.current;
        if (!alive)
        {
            if (cam != null && cam.tag == "MainCamera")
                alive = true;
        }
        if (alive && !dead)
        {
            if (cam != null && cam.tag == "DeathCamera")
            {
                dead = true;
                text.enabled = true;
            }
        }
    }
}