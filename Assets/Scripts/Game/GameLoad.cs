using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoad : MonoBehaviour {

	void Start() {
        MapFileReader reader = new MapFileReader("mapsave");
        reader.Read(null, GetComponent<VoxelArray>());
	}

    public void Close()
    {
        SceneManager.LoadScene("editScene");
    }
	
}
