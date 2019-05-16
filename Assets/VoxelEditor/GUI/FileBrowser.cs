using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileBrowser : GUIPanel
{
    public System.Action<string> fileAction;
    public string path;
    private List<string> fileList = new List<string>();

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            1152, safeRect.height * .9f);
    }

    void Start()
    {
        UpdateFileList();
    }

    private void UpdateFileList()
    {
        scroll = Vector2.zero;
        string[] files = Directory.GetFileSystemEntries(path);
        fileList.Clear();
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            if (!name.StartsWith("."))
                fileList.Add(name);
        }
    }

    public override void WindowGUI()
    {
        GUILayout.Label(path);
        scroll = GUILayout.BeginScrollView(scroll);
        if (GUIUtils.HighlightedButton("Back"))
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
