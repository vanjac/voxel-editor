using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoad : MonoBehaviour {

    void Start()
    {
        MapFileReader reader = new MapFileReader(SelectedMap.GetSelectedMapName());
        reader.Read(null, GetComponent<VoxelArray>(), false);
	}

    public void Close()
    {
        SceneManager.LoadScene("editScene");
    }
	
}
