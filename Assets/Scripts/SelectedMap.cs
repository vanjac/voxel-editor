using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedMap : MonoBehaviour {

    public string mapName;

    void Start () {
        DontDestroyOnLoad(this);
    }
}
