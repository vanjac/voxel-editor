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
    public TouchListener touchListener;

    public void Load()
    {
        StartCoroutine(LoadCoroutine());
    }

    private IEnumerator LoadCoroutine()
    {
        yield return null;
        string mapName = SelectedMap.Instance().mapName;
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
            string message = "There were some issues with reading the world:\n\n  •  " +
                string.Join("\n  •  ", warnings.ToArray());
            LargeMessageGUI.ShowLargeMessageDialog(loadingGUI.gameObject, message);
        }

        if (!PlayerPrefs.HasKey("last_editScene_version"))
        {
            var dialog = loadingGUI.gameObject.AddComponent<DialogGUI>();
            dialog.message = "This is your first time using the app. Would you like a tutorial?";
            dialog.yesButtonText = "Yes";
            dialog.noButtonText = "No";
            dialog.yesButtonHandler = () =>
            {
                var tutorialGUI = dialog.gameObject.AddComponent<TutorialGUI>();
                tutorialGUI.voxelArray = voxelArray;
                tutorialGUI.touchListener = touchListener;
                tutorialGUI.StartTutorial(Tutorials.INTRO_TUTORIAL);
            };
        }
        PlayerPrefs.SetString("last_editScene_version", Application.version);
    }

    public void Save()
    {
        if (!voxelArray.unsavedChanges)
        {
            Debug.unityLogger.Log("EditorFile", "No unsaved changes");
            return;
        }
        Debug.unityLogger.Log("EditorFile", "Saving...");
        MapFileWriter writer = new MapFileWriter(SelectedMap.Instance().mapName);
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
        else if (ShareMap.CatchSharedFile())
        {
            Save();
            SceneManager.LoadScene("fileReceiveScene");
        }
    }
}
