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
        Debug.Log("Loading...");
        MapFileReader reader = new MapFileReader("mapsave");
        reader.Read(cameraPivot, voxelArray);
    }

    public void Save()
    {
        Debug.Log("Saving...");
        MapFileWriter writer = new MapFileWriter("mapsave");
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
