using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShareListener : MonoBehaviour
{
    void Start()
    {
        CheckSharedFile();
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused)
            CheckSharedFile();
    }

    private void CheckSharedFile()
    {
        if (ShareMap.CatchSharedFile())
            SceneManager.LoadScene("fileReceiveScene");
    }
}