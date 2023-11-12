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
            if (lastVersion.EndsWith("b"))
                lastVersion = lastVersion.Substring(0, lastVersion.Length - 1);
            if (CompareVersions(lastVersion, "1.3.5") == -1)
            {
                LargeMessageGUI.ShowLargeMessageDialog(guiGameObject, "N-Space has been updated!"
                    + " Check out the <b>Doors</b> Demo World in the help menu to see the new features.\n\n"
                    + "•  New <b>Scale</b> behavior to change size of objects/substances\n"
                    + "•  You can change the <b>Pivot</b> point of substances for rotation/scaling\n"
                    + "•  Lights are visible in the editor, even when not selected\n"
                    + "•  Objects can be placed inside walls or outside bounds\n"
                    + "•  Fixed lag caused by larger substances\n"
                    + "•  Button in the bottom right toggles pan/orbit with two fingers\n"
                    + "... and some more improvements / fixes!");
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
            int numA = int.Parse(aNums[i]);
            int numB = int.Parse(bNums[i]);
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

    public bool Save(bool allowPopups = true)
    {
        if (!voxelArray.unsavedChanges)
        {
            Debug.unityLogger.Log("EditorFile", "No unsaved changes");
            return true;
        }
        if (voxelArray.IsEmpty())
        {
            Debug.Log("World is empty! File will not be written.");
            return true;
        }
        string savePath = SelectedWorld.GetSavePath();
        try
        {
            if (System.IO.File.Exists(savePath))
            {
                MessagePackWorldWriter.Write(WorldFiles.GetTempPath(),
                    cameraPivot, voxelArray);
                WorldFiles.RestoreTempFile(savePath);
            }
            else
            {
                MessagePackWorldWriter.Write(savePath, cameraPivot, voxelArray);
            }
            voxelArray.unsavedChanges = false;
            return true;
        }
        catch (System.Exception e)
        {
            if (allowPopups)
            {
                string message = "An error occurred while saving the file. "
                    + "Please send me an email about this, and include a screenshot "
                    + "of this message. chroma@chroma.zone\n\n"
                    + e.ToString();
                var dialog = LargeMessageGUI.ShowLargeMessageDialog(GUIPanel.GuiGameObject, message);
                dialog.closeHandler = () => SceneManager.LoadScene(Scenes.MENU);
                voxelArray.unsavedChanges = false;
            }
            return false;
        }
    }

    public void Play()
    {
        Debug.unityLogger.Log("EditorFile", "Play");
        if (Save())
            SceneManager.LoadScene(Scenes.GAME);
    }

    public void Close()
    {
        Debug.unityLogger.Log("EditorFile", "Close");
        if (Save())
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
        Save(allowPopups: false);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        Debug.unityLogger.Log("EditorFile", "OnApplicationPause(" + pauseStatus + ")");
        if (pauseStatus)
            Save(allowPopups: false);
        else if (ShareMap.FileWaitingToImport())
        {
            if (importWorldHandler == null)
            {
                if (Save())
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
                    DialogGUI.ShowMessageDialog(GUIPanel.GuiGameObject, "An error occurred while reading the file.");
                    Debug.LogError(e);
                    stream?.Dispose();
                    ShareMap.ClearFileWaitingToImport();
                }
            }
        }
    }
}
