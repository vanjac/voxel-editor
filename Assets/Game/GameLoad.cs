using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoad : MonoBehaviour
{
    public LoadingGUI loadingGUI;

    void Start()
    {
        StartCoroutine(LoadCoroutine());
    }

    private IEnumerator LoadCoroutine()
    {
        yield return null;
        try
        {
            ReadWorldFile.Read(SelectedWorld.Instance().worldPath,
                null, GetComponent<VoxelArray>(), false);
        }
        catch (MapReadException e)
        {
            var dialog = loadingGUI.gameObject.AddComponent<DialogGUI>();
            dialog.message = e.Message;
            dialog.yesButtonText = "Close";
            dialog.yesButtonHandler = () =>
            {
                Close("menuScene");
            };
            Debug.Log(e.InnerException);
            yield break;
        }
        finally
        {
            Destroy(loadingGUI);
        }
    }

    public void Close(string scene)
    {
        StartCoroutine(CloseCoroutine(scene));
    }

    private IEnumerator CloseCoroutine(string scene)
    {
        Time.timeScale = 1; // unpause; also necessary to prevent freezing
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
        if (!paused && ShareMap.FileWaitingToImport())
        {
            Close("fileReceiveScene");
        }
    }
}
