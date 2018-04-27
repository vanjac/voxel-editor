using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FileReceive : MonoBehaviour
{
    public GUISkin guiSkin;

    void Start()
    {
        ShareMap.MarkIntentUsedAndroid();

        TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
        inputDialog.prompt = "Enter name to save imported map as...";
        inputDialog.handler = ImportMap;
        inputDialog.guiSkin = guiSkin;
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
        string newPath = MenuGUI.GetMapPath(name);
        if (File.Exists(newPath))
        {
            var dialog = DialogGUI.ShowMessageDialog(gameObject, "A map with that name already exists.");
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
            var dialog = DialogGUI.ShowMessageDialog(gameObject, "Error importing file");
            dialog.yesButtonHandler = Close;
        }
    }
}