using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorFile : MonoBehaviour
{
    public VoxelArray voxelArray;
    public Transform cameraPivot;

    public void Load()
    {
        string mapName = SelectedMap.GetSelectedMapName();
        Debug.Log("Loading " + mapName);
        MapFileReader reader = new MapFileReader(mapName);
        reader.Read(cameraPivot, voxelArray, true);
    }

    public void Save()
    {
        Debug.Log("Saving...");
        MapFileWriter writer = new MapFileWriter(SelectedMap.GetSelectedMapName());
        writer.Write(cameraPivot, voxelArray);
    }

    public void LoadScene(string name)
    {
        Save();
        SceneManager.LoadScene(name);
    }

    void OnEnable()
    {
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
