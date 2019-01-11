using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FileReceiveGUI : GUIPanel
{
    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(safeRect.xMin, safeRect.yMin, safeRect.width, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

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
        inputDialog.prompt = "Enter name for imported world...";
        inputDialog.handler = ImportWorld;
        inputDialog.cancelHandler = DestroyThis;
    }

    void OnDestroy()
    {
        ShareMap.ClearFileWaitingToImport();
        SceneManager.LoadScene(Scenes.MENU);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        ActionBarGUI.ActionBarLabel("Importing file...");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void ImportWorld(string name)
    {
        if (name.Length == 0)
        {
            Destroy(this);
            return;
        }
        string newPath = WorldFiles.GetNewWorldPath(name);
        if (File.Exists(newPath))
        {
            var dialog = DialogGUI.ShowMessageDialog(gameObject, "A world with that name already exists.");
            dialog.yesButtonHandler = DestroyThis;
            return;
        }

        try
        {
            ShareMap.ImportSharedFile(newPath);
            Destroy(this);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            var dialog = DialogGUI.ShowMessageDialog(gameObject, "Error importing world");
            dialog.yesButtonHandler = DestroyThis;
        }
    }
}