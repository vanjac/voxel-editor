using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorFile : MonoBehaviour
{
    public static EditorFile instance;
    public LoadingGUI loadingGUI;
    public List<MonoBehaviour> enableOnLoad;

    public VoxelArrayEditor voxelArray;
    public Transform cameraPivot;
    public TouchListener touchListener;

    // importWorldHandler MUST dispose stream and call ShareMap.ClearFileWaitingToImport() when finished
    public System.Action<System.IO.Stream> importWorldHandler;

    void Start()
    {
        instance = this;
    }

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
            warnings = ReadWorldFile.Read(SelectedWorld.GetLoadStream(),
                cameraPivot, voxelArray, true);
        }
        catch (MapReadException e)
        {
            var dialog = guiGameObject.AddComponent<DialogGUI>();
            dialog.message = e.FullMessage;
            dialog.yesButtonText = "Close";
            dialog.yesButtonHandler = () =>
            {
                voxelArray.unsavedChanges = false;
                Close();
            };
            Destroy(loadingGUI);
            Debug.LogError(e);
            yield break;
        }
        // reading the file creates new voxels which sets the unsavedChanges flag
        // and clears existing voxels which sets the selectionChanged flag
        voxelArray.unsavedChanges = false;
        voxelArray.selectionChanged = false;

        Destroy(loadingGUI);
        foreach (MonoBehaviour b in enableOnLoad)
            b.enabled = true;

        if (PlayerPrefs.HasKey("last_editScene_version"))
        {
            string lastVersion = PlayerPrefs.GetString("last_editScene_version");
            if (CompareVersions(lastVersion, "1.3.3") == -1)
            {
                LargeMessageGUI.ShowLargeMessageDialog(guiGameObject, "N-Space has been updated!\n\n"
                    + "•  You can now import custom textures from your photos library, "
                    + "and change the color of any built-in material or overlay.\n"
                    + "•  Two new behaviors have been added: <b>Character</b> and <b>Look At</b>.\n"
                    + "•  Move behavior now has an option for \"local\" coordinates, or for choosing a random direction.\n"
                    + "... and much more! Check out the new Demo Worlds (Character AI, Hovercraft, and Shapes) for examples.");
            }
            else if (CompareVersions(lastVersion, "1.3.0") == -1)
            {
                LargeMessageGUI.ShowLargeMessageDialog(guiGameObject, "<b>Version 1.3.0 update</b>\n\n"
                    + "N-Space has been updated with a new behavior for sound effects and music. Try it out!\n\n"
                    + "Also, check the main menu for links to video tutorials and a subreddit.");
            }
            else if (CompareVersions(lastVersion, "1.3.2") == -1)
            {
                LargeMessageGUI.ShowLargeMessageDialog(guiGameObject, "N-Space has been updated!\n\n"
                    + "3 new behaviors: Carryable, 3D Sound, and Score\n"
                    + "2 new sensors: Random Pulse, and Check Score\n"
                    + "There are now built-in sound effects for walking on different surfaces\n\n"
                    + "... and many more improvements!");
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
        MessagePackWorldWriter.Write(SelectedWorld.GetSavePath(),
            cameraPivot, voxelArray);
        voxelArray.unsavedChanges = false;
    }

    public void Play()
    {
        Debug.unityLogger.Log("EditorFile", "Play");
        Save();
        SceneManager.LoadScene(Scenes.GAME);
    }

    public void Close()
    {
        Debug.unityLogger.Log("EditorFile", "Close");
        Save();
        SceneManager.LoadScene(Scenes.MENU);
    }

    public void Revert()
    {
        Debug.unityLogger.Log("EditorFile", "Revert");
        SceneManager.LoadScene(Scenes.EDITOR);
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
            if (importWorldHandler == null)
            {
                Save();
                SceneManager.LoadScene(Scenes.FILE_RECEIVE);
            }
            else
            {
                System.IO.Stream stream = null;
                try
                {
                    stream = ShareMap.GetImportStream();
                    importWorldHandler(stream);
                }
                catch (System.Exception e)
                {
                    DialogGUI.ShowMessageDialog(GUIManager.guiGameObject, "An error occurred while reading the file.");
                    Debug.LogError(e);
                    if (stream != null)
                        stream.Dispose();
                    ShareMap.ClearFileWaitingToImport();
                }
            }
        }
    }
}
