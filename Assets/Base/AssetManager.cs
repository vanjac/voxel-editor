using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    private static AssetManager instance;

    private bool unloadUnusedAssets = true;
    private float lastUnloadTime = -99;

    public static void UnusedAssets()
    {
        if (instance == null)
            return;
        instance.unloadUnusedAssets = true;
    }

    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (unloadUnusedAssets && Time.time - lastUnloadTime > 5)
        {
            unloadUnusedAssets = false;
            lastUnloadTime = Time.time;
            Resources.UnloadUnusedAssets();
        }
    }
}