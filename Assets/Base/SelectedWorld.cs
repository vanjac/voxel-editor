using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedWorld : MonoBehaviour
{
    public string worldPath;

    public static SelectedWorld Instance()
    {
        GameObject go = GameObject.Find("SelectedWorld");
        if (go == null)
        {
            go = new GameObject("SelectedWorld");
            DontDestroyOnLoad(go);
            var selectedWorld = go.AddComponent<SelectedWorld>();
            selectedWorld.worldPath = WorldFiles.GetNewWorldPath("mapsave");
            return selectedWorld;
        }
        else
        {
            return go.GetComponent<SelectedWorld>();
        }
    }
}
