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
        var guiGameObject = loadingGUI.gameObject;

        List<string> warnings;
        try
        {
            warnings = ReadWorldFile.Read(SelectedWorld.Instance().worldPath,
                cameraPivot, voxelArray, true);
        }
        catch (MapReadException e)
        {
            var dialog = guiGameObject.AddComponent<DialogGUI>();
            dialog.message = e.Message;
            dialog.yesButtonText = "Close";
            dialog.yesButtonHandler = () =>
            {
                voxelArray.unsavedChanges = false;
                Close();
            };
            Destroy(loadingGUI);
            Debug.Log(e);
            yield break;
        }
        // reading the file creates new voxels which sets the unsavedChanges flag
        // and clears existing voxels which sets the selectionChanged flag
        voxelArray.unsavedChanges = false;
        voxelArray.selectionChanged = false;

        Destroy(loadingGUI);
        foreach (MonoBehaviour b in enableOnLoad)
            b.enabled = true;

        if (!PlayerPrefs.HasKey("last_editScene_version"))
        {
            var dialog = guiGameObject.AddComponent<DialogGUI>();
            dialog.message = "This is your first time using the app. Would you like a tutorial?";
            dialog.yesButtonText = "Yes";
            dialog.noButtonText = "No";
            dialog.yesButtonHandler = () =>
            {
                TutorialGUI.StartTutorial(Tutorials.INTRO_TUTORIAL, guiGameObject, voxelArray, touchListener);
            };
        }
        else
        {
            string lastVersion = PlayerPrefs.GetString("last_editScene_version");
            if (CompareVersions(lastVersion, "1.2.0") == -1)
            {
                var dialog = guiGameObject.AddComponent<DialogGUI>();
                dialog.message = "N-Space has been updated with the ability to bevel edges! Would you like a tutorial?";
                dialog.yesButtonText = "Yes";
                dialog.noButtonText = "No";
                dialog.yesButtonHandler = () =>
                {
                    TutorialGUI.StartTutorial(Tutorials.BEVEL_TUTORIAL, guiGameObject, voxelArray, touchListener);
                };
            }
        }
        PlayerPrefs.SetString("last_editScene_version", Application.version);

        if (warnings.Count > 0)
        {
            // avoids a bug where two dialogs created on the same frame will put the unfocused one on top
            // for some reason it's necessary to wait two frames
            yield return null;
            yield return null;
            string message = "There were some issues with reading the world:\n\n  •  " +
                string.Join("\n  •  ", warnings.ToArray());
            LargeMessageGUI.ShowLargeMessageDialog(guiGameObject, message);
        }
    }

    // 1: a is greater; -1: b is creater; 0: equal
    private static int CompareVersions(string a, string b)
    {
        string[] aNums = a.Split('.');
        string[] bNums = b.Split('.');
        for (int i = 0; i < aNums.Length; i++)
        {
            if (i >= bNums.Length)
                return 1;
            int numA = System.Int32.Parse(aNums[i]);
            int numB = System.Int32.Parse(bNums[i]);
            if (numA > numB)
                return 1;
            else if (numB > numA)
                return -1;
        }
        if (bNums.Length > aNums.Length)
            return -1;
        else
            return 0;
    }

    public void Save()
    {
        if (!voxelArray.unsavedChanges)
        {
            Debug.unityLogger.Log("EditorFile", "No unsaved changes");
            return;
        }
        MessagePackWorldWriter.Write(SelectedWorld.Instance().worldPath,
            cameraPivot, voxelArray);
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
        else if (ShareMap.FileWaitingToImport())
        {
            Save();
            SceneManager.LoadScene("fileReceiveScene");
        }
    }
}
