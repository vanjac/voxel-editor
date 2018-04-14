using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorFile : MonoBehaviour
{
    public LoadingGUI loadingGUI;
    public List<MonoBehaviour> enableOnLoad;

    public VoxelArrayEditor voxelArray;
    public Transform cameraPivot;

    public void Load()
    {
        StartCoroutine(LoadCoroutine());
    }

    private IEnumerator LoadCoroutine()
    {
        yield return null;
        string mapName = SelectedMap.GetSelectedMapName();
        Debug.unityLogger.Log("EditorFile", "Loading " + mapName);
        MapFileReader reader = new MapFileReader(mapName);
        List<string> warnings;
        try
        {
            warnings = reader.Read(cameraPivot, voxelArray, true);
        }
        catch (MapReadException e)
        {
            var dialog = loadingGUI.gameObject.AddComponent<DialogGUI>();
            dialog.message = e.Message;
            dialog.yesButtonText = "Close";
            dialog.yesButtonHandler = () =>
            {
                voxelArray.unsavedChanges = false;
                Close();
            };
            // fix issue where message dialog doesn't use correct skin:
            dialog.guiSkin = loadingGUI.guiSkin;
            Destroy(loadingGUI);
            Debug.Log(e.InnerException);
            yield break;
        }
        // reading the file creates new voxels which sets the unsavedChanges flag
        // and clears existing voxels which sets the selectionChanged flag
        voxelArray.unsavedChanges = false;
        voxelArray.selectionChanged = false;

        Destroy(loadingGUI);
        foreach (MonoBehaviour b in enableOnLoad)
            b.enabled = true;
        if (warnings.Count > 0)
        {
            string message = "There were some issues with reading the map:\n\n  •  " +
                string.Join("\n  •  ", warnings.ToArray());
            LargeMessageGUI.ShowLargeMessageDialog(loadingGUI.gameObject, message);
        }
    }

    public void Save()
    {
        if (!voxelArray.unsavedChanges)
        {
            Debug.unityLogger.Log("EditorFile", "No unsaved changes");
            return;
        }
        Debug.unityLogger.Log("EditorFile", "Saving...");
        MapFileWriter writer = new MapFileWriter(SelectedMap.GetSelectedMapName());
        writer.Write(cameraPivot, voxelArray);
        voxelArray.unsavedChanges = false;
    }

    public void Play()
    {
        Debug.unityLogger.Log("EditorFile", "Play");
        Save();
        SceneManager.LoadScene("playScene");
    }

    public void Close()
    {
        Debug.unityLogger.Log("EditorFile", "Close");
        Save();
        SceneManager.LoadScene("menuScene");
    }

    void OnEnable()
    {
        Debug.unityLogger.Log("EditorFile", "OnEnable()");
        Load();
    }

    void OnApplicationQuit()
    {
        Debug.unityLogger.Log("EditorFile", "OnApplicationQuit()");
        Save();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        Debug.unityLogger.Log("EditorFile", "OnApplicationPause(" + pauseStatus + ")");
        if (pauseStatus)
            Save();
    }
}
