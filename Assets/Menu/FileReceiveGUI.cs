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

    void Start()
    {
        TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
        inputDialog.prompt = "Enter name for imported world...";
        inputDialog.handler = ImportMap;
        inputDialog.cancelHandler = Close;
    }

    void OnDestroy()
    {
        ShareMap.ClearFileWaitingToImport();
    }

    private void Close()
    {
        SceneManager.LoadScene("menuScene");
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        ActionBarGUI.ActionBarLabel("Importing file...");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void ImportMap(string name)
    {
        if (name.Length == 0)
        {
            Close();
            return;
        }
        string newPath = WorldFiles.GetFilePath(name);
        if (File.Exists(newPath))
        {
            var dialog = DialogGUI.ShowMessageDialog(gameObject, "A world with that name already exists.");
            dialog.yesButtonHandler = Close;
            return;
        }

        try
        {
            ShareMap.ImportSharedFile(newPath);
            Close();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            var dialog = DialogGUI.ShowMessageDialog(gameObject, "Error importing world");
            dialog.yesButtonHandler = Close;
        }
    }
}