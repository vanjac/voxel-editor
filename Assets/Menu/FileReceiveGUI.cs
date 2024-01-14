using UnityEngine;
using UnityEngine.SceneManagement;

public class FileReceiveGUI : GUIPanel
{
    bool openingWorld = false;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        new Rect(safeRect.xMin, safeRect.yMin, safeRect.width, 0);

    public override GUIStyle GetStyle() => GUIStyle.none;

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    private void DestroyThis() // to use as callback
    {
        Destroy(this);
    }

    void Start()
    {
        TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
        inputDialog.prompt = StringSet.ImportNamePrompt;
        inputDialog.handler = ImportWorld;
        inputDialog.cancelHandler = DestroyThis;
    }

    void OnDestroy()
    {
        ShareMap.ClearFileWaitingToImport();
        if (!openingWorld)
            SceneManager.LoadScene(Scenes.MENU);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        ActionBarGUI.ActionBarLabel(StringSet.ImportingFile);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void ImportWorld(string name)
    {
        if (!WorldFiles.ValidateName(name, out string errorMessage))
        {
            if (errorMessage != null)
            {
                var dialog = DialogGUI.ShowMessageDialog(gameObject, errorMessage);
                dialog.yesButtonHandler = DestroyThis;
            }
            else
            {
                Destroy(this);
            }
            return;
        }

        string newPath = WorldFiles.GetNewWorldPath(name);
        try
        {
            ShareMap.ImportSharedFile(newPath);
            MenuGUI.OpenWorld(newPath, Scenes.EDITOR);
            openingWorld = true;
            Destroy(this);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            var dialog = DialogGUI.ShowMessageDialog(gameObject, StringSet.ImportError);
            dialog.yesButtonHandler = DestroyThis;
        }
    }
}