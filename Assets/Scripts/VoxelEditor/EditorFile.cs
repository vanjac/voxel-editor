using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorFile : MonoBehaviour
{
    public VoxelArray voxelArray;
    public Transform cameraPivot;
    string mapName = "mapsave";

    public void Load()
    {
        Debug.Log("Loading " + mapName);
        MapFileReader reader = new MapFileReader(mapName);
        reader.Read(cameraPivot, voxelArray);
    }

    public void Save()
    {
        Debug.Log("Saving...");
        MapFileWriter writer = new MapFileWriter(mapName);
        writer.Write(cameraPivot, voxelArray);
    }

    public void LoadScene(string name)
    {
        Save();
        SceneManager.LoadScene(name);
    }

    void OnEnable()
    {
        GameObject selectedMap = GameObject.Find("SelectedMap");
        if (selectedMap != null)
        {
            mapName = selectedMap.GetComponent<SelectedMap>().mapName;
        }
        Load();
    }

    void OnApplicationQuit()
    {
        Save();
    }

    void OnApplicationPause()
    {
        Save();
    }
}
