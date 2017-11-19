using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorFile : MonoBehaviour
{
    public VoxelArray voxelArray;
    public Transform cameraPivot;

    void OnEnable()
    {
        Debug.Log("Loading...");
        MapFileReader reader = new MapFileReader("mapsave");
        reader.Read(cameraPivot, voxelArray);
    }

    void OnDisable()
    {
        Debug.Log("Saving...");
        MapFileWriter writer = new MapFileWriter("mapsave");
        writer.Write(cameraPivot, voxelArray);
    }

    void OnApplicationPause()
    {
        OnDisable();
    }
}
