using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGUI : GUIPanel
{
    public TextAsset defaultMap;

    private List<string> mapFiles = new List<string>();
    private TouchScreenKeyboard nameKeyboard;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .25f, height * .2f, width * .5f, height * .6f);
    }

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    void Start()
    {
        UpdateMapList();
    }

    public override void WindowGUI()
    {
        if (nameKeyboard != null)
        {
            if (nameKeyboard.status == TouchScreenKeyboard.Status.Done)
                NewMap(nameKeyboard.text);
            if (nameKeyboard.status != TouchScreenKeyboard.Status.Visible)
                nameKeyboard = null;
        }
        if (GUILayout.Button("New...") && nameKeyboard == null)
        {
            nameKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.ASCIICapable,
                false, false, false, false, // autocorrect, multiline, password, alert mode
                "Enter new map name...");
        }
        scroll = GUILayout.BeginScrollView(scroll);
        bool updateMapList = false;
        foreach (string fileName in mapFiles)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(fileName))
                OpenMap(fileName);
            if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
            {
                File.Delete(GetMapPath(fileName));
                updateMapList = true;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        if (updateMapList)
            UpdateMapList();
    }

    private void OpenMap(string name)
    {
        GameObject selectedMap = GameObject.Find("SelectedMap");
        if (selectedMap == null)
        {
            selectedMap = new GameObject("SelectedMap");
            selectedMap.AddComponent<SelectedMap>();
        }
        selectedMap.GetComponent<SelectedMap>().mapName = name;
        SceneManager.LoadScene("editScene");
    }

    private void NewMap(string name)
    {
        if (name.Length == 0)
            return;
        string filePath = GetMapPath(name);
        if (File.Exists(filePath))
            return;
        using (FileStream fileStream = File.Create(filePath))
        {
            using (var sw = new StreamWriter(fileStream))
            {
                sw.Write(defaultMap.text);
                sw.Flush();
            }
        }
        UpdateMapList();
    }

    private string GetMapPath(string name)
    {
        return Application.persistentDataPath + "/" + name + ".json";
    }

    private void UpdateMapList()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath);
        mapFiles.Clear();
        foreach (string name in files)
            mapFiles.Add(Path.GetFileNameWithoutExtension(name));
    }
}