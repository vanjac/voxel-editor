using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoad : MonoBehaviour {

    void Start()
    {
        string name = "mapsave";
        GameObject selectedMap = GameObject.Find("SelectedMap");
        if (selectedMap != null)
        {
            name = selectedMap.GetComponent<SelectedMap>().mapName;
        }
        MapFileReader reader = new MapFileReader(name);
        reader.Read(null, GetComponent<VoxelArray>());
	}

    public void Close()
    {
        SceneManager.LoadScene("editScene");
    }
	
}
