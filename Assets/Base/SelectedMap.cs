using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedMap : MonoBehaviour
{
    public string mapName = "mapsave";

    public static SelectedMap Instance()
    {
        GameObject selectedMap = GameObject.Find("SelectedMap");
        if (selectedMap == null)
        {
            selectedMap = new GameObject("SelectedMap");
            DontDestroyOnLoad(selectedMap);
            return selectedMap.AddComponent<SelectedMap>();
        }
        else
        {
            return selectedMap.GetComponent<SelectedMap>();
        }
    }
}
