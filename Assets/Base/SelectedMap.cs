using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedMap : MonoBehaviour
{
    public string mapName;

    void Start () {
        DontDestroyOnLoad(this);
    }

    public static string GetSelectedMapName()
    {
        GameObject selectedMap = GameObject.Find("SelectedMap");
        if (selectedMap != null)
            return selectedMap.GetComponent<SelectedMap>().mapName;
        else
            return "mapsave";
    }
}
