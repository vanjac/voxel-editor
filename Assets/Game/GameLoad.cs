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
        MapFileReader reader = new MapFileReader(SelectedMap.Instance().mapName);
        try
        {
            reader.Read(null, GetComponent<VoxelArray>(), false);
        }
        catch (MapReadException)
        {
            SceneManager.LoadScene("editScene"); // TODO: this is a very bad solution
        }
        loadingText.enabled = false;
    }

    public void Close()
    {
        StartCoroutine(CloseCoroutine());
    }

    private IEnumerator CloseCoroutine(string scene=null)
    {
        yield return null;
        foreach (var substance in GetComponent<VoxelArray>().GetComponentsInChildren<SubstanceComponent>())
        {
            Rigidbody rigidBody = substance.GetComponent<Rigidbody>();
            if (rigidBody != null)
                // this avoids long freezes when unloading the scene. I'm not sure why, but my guess is that
                // as each voxel child is destroyed it causes the rigidbody to do some calculations to update.
                // so we make sure the rigidbodies are destroyed before anything else.
                Destroy(rigidBody);
        }
        yield return null;
        if (scene == null)
            SceneManager.LoadScene(SelectedMap.Instance().returnFromPlayScene);
        else
            SceneManager.LoadScene(scene);
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            Close();
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused && ShareMap.CatchSharedFile())
        {
            StartCoroutine(CloseCoroutine("fileReceiveScene"));
        }
    }
}
