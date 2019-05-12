using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileBrowser : GUIPanel
{
    public System.Action<string> fileAction;

    private string path;
    private string[] fileList = new string[0];

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            1152, safeRect.height * .9f);
    }

    void Start()
    {
        path = Application.persistentDataPath;
        UpdateFileList();
    }

    private void UpdateFileList()
    {
        fileList = Directory.GetFileSystemEntries(path);
        for (int i = 0; i < fileList.Length; i++)
            fileList[i] = Path.GetFileName(fileList[i]);
        scroll = Vector2.zero;
    }

    public override void WindowGUI()
    {
        GUILayout.Label(path);
        scroll = GUILayout.BeginScrollView(scroll);
        if (GUILayout.Button(".."))
        {
            path = Path.GetDirectoryName(path);
            UpdateFileList();
        }
        foreach (string fileName in fileList)
        {
            if (GUILayout.Button(fileName))
            {
                string fullPath = path + '/' + fileName;
                if (Directory.Exists(fullPath))
                {
                    path = fullPath;
                    UpdateFileList();
                }
                else
                {
                    fileAction(fullPath);
                    Destroy(this);
                }
            }
        }
        GUILayout.EndScrollView();
    }
}
