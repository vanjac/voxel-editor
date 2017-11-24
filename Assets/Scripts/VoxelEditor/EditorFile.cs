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
        // reading the file creates new voxels which sets the unsavedChanges flag
        voxelArray.unsavedChanges = false;
    }

    public void Save()
    {
        if (!voxelArray.unsavedChanges)
        {
            Debug.Log("No unsaved changes");
            return;
        }
        Debug.Log("Saving...");
        MapFileWriter writer = new MapFileWriter(SelectedMap.GetSelectedMapName());
        writer.Write(cameraPivot, voxelArray);
        voxelArray.unsavedChanges = false;
    }

    public void LoadScene(string name)
    {
        Debug.Log("LoadScene(" + name + ")");
        Save();
        SceneManager.LoadScene(name);
    }

    void OnEnable()
    {
        Debug.Log("OnEnable()");
        Load();
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit()");
        Save();
    }

    void OnApplicationPause()
    {
        Debug.Log("OnApplicationPause()");
        Save();
    }
}
