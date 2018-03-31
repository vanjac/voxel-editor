using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoad : MonoBehaviour
{
    public UnityEngine.UI.Text loadingText;

    void Start()
    {
        StartCoroutine(LoadCoroutine());
    }

    private IEnumerator LoadCoroutine()
    {
        yield return null;
        MapFileReader reader = new MapFileReader(SelectedMap.GetSelectedMapName());
        reader.Read(null, GetComponent<VoxelArray>(), false);
        loadingText.enabled = false;
    }

    public void Close()
    {
        SceneManager.LoadScene(SelectedMap.GetReturnFromPlayScene());
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            Close();
    }
}
