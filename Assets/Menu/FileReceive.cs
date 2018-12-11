using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FileReceive : MonoBehaviour
{
    void Start()
    {
        ShareMap.MarkIntentUsedAndroid();

        TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
        inputDialog.prompt = "Enter name for imported world...";
        inputDialog.handler = ImportMap;
        inputDialog.cancelHandler = Close;
    }

    private void Close()
    {
        SceneManager.LoadScene("menuScene");
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
            using (FileStream fileStream = File.Create(newPath))
            {
                ShareMap.ReadSharedURLAndroid(fileStream);
            }
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