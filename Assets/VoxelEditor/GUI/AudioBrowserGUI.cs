using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public interface AudioPlayer
{
    void Stop();
}

public delegate AudioPlayer AudioPlayerFactory(byte[] data);

public class AudioBrowserGUI : GUIPanel
{
    public AudioPlayerFactory playerFactory;
    public System.Action<string> fileAction;
    public string path;
    public string extensions;
    private List<string> fileList = new List<string>();
    private AudioPlayer playingAudio;
    private string playingFile;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            1152, safeRect.height * .9f);
    }

    void Start()
    {
        UpdateFileList();
    }

    void OnDestroy()
    {
        if (playingAudio != null)
            playingAudio.Stop();
    }

    private void UpdateFileList()
    {
        if (playingAudio != null)
        {
            playingAudio.Stop();
            playingAudio = null;
            playingFile = null;
        }

        scroll = Vector2.zero;
        string[] files = Directory.GetFileSystemEntries(path);
        fileList.Clear();
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            if (!name.StartsWith(".") && extensions.Contains(Path.GetExtension(name)))
                fileList.Add(name);
        }
    }

    public override void WindowGUI()
    {
        GUILayout.Label(Path.GetFileName(path) + ":");
        scroll = GUILayout.BeginScrollView(scroll);
        if (GUIUtils.HighlightedButton("Back", GUIStyleSet.instance.buttonLarge))
        {
            path = Path.GetDirectoryName(path);
            UpdateFileList();
        }
        bool update = false;
        foreach (string fileName in fileList)
        {
            string fullPath = path + '/' + fileName;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(fileName, GUIStyleSet.instance.buttonLarge))
            {
                if (Directory.Exists(fullPath))
                {
                    path = fullPath;
                    update = true;
                }
                else
                {
                    fileAction(fullPath);
                    Destroy(this);
                }
            }
            if (fileName.Contains(".") && GUIUtils.HighlightedButton(
                GUIIconSet.instance.playAudio,
                GUIStyleSet.instance.buttonLarge,
                playingFile == fileName,
                GUILayout.ExpandWidth(false)))
            {
                if (playingAudio != null)
                    playingAudio.Stop();
                if (playingFile == fileName)
                {
                    playingAudio = null;
                    playingFile = null;
                }
                else
                {
                    var data = System.IO.File.ReadAllBytes(fullPath);
                    playingAudio = playerFactory(data);
                    playingFile = fileName;
                }
            }
            GUILayout.EndHorizontal();
        }
        if (update)
            UpdateFileList();
        GUILayout.EndScrollView();
    }
}
