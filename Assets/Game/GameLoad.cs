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

    public void Close(string scene)
    {
        StartCoroutine(CloseCoroutine(scene));
    }

    private IEnumerator CloseCoroutine(string scene)
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
        SceneManager.LoadScene(scene);
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused && ShareMap.CatchSharedFile())
        {
            Close("fileReceiveScene");
        }
    }
}
