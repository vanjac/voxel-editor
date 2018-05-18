using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedMap : MonoBehaviour
{
    public string mapName = "mapsave";
    public string returnFromPlayScene = "editScene";

    void Start ()
    {
        DontDestroyOnLoad(this);
    }

    public static SelectedMap Instance()
    {
        GameObject selectedMap = GameObject.Find("SelectedMap");
        if (selectedMap == null)
        {
            selectedMap = new GameObject("SelectedMap");
            return selectedMap.AddComponent<SelectedMap>();
        }
        else
        {
            return selectedMap.GetComponent<SelectedMap>();
        }
    }
}
