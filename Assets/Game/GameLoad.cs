using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoad : MonoBehaviour
{
    public ReflectionProbe reflectionProbe;
    private bool firstUpdate = true;

    void Start()
    {
        MapFileReader reader = new MapFileReader(SelectedMap.GetSelectedMapName());
        reader.Read(null, GetComponent<VoxelArray>(), false);
    }

    public void Close()
    {
        SceneManager.LoadScene("editScene");
    }

    void Update()
    {
        if (firstUpdate)
        {
            firstUpdate = false;
            reflectionProbe.RenderProbe();
        }
        if (Input.GetButtonDown("Cancel"))
            Close();
    }
}
