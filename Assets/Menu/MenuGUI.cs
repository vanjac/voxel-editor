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
        foreach (string fileName in mapFiles)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(fileName))
                OpenMap(fileName);
            if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
            {
                FileDropdownGUI dropdown = gameObject.AddComponent<FileDropdownGUI>();
                dropdown.fileName = fileName;
                dropdown.location = Input.GetTouch(0).position;
                dropdown.location.y = Screen.height - dropdown.location.y;
                dropdown.location /= scaleFactor;
                dropdown.location -= new Vector2(30, 30);
                dropdown.handler = () =>
                {
                    UpdateMapList();
                };
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
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

    public static string GetMapPath(string name)
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

public class FileDropdownGUI : GUIPanel
{
    public delegate void UpdateMapListHandler();

    public Vector2 location = new Vector2(100, 100);
    public string fileName;
    public UpdateMapListHandler handler;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(location.x, location.y, width * .2f, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void WindowGUI()
    {
        if (GUILayout.Button("Delete"))
        {
            DialogGUI dialog = gameObject.AddComponent<DialogGUI>();
            dialog.message = "Are you sure you want to delete " + fileName + "?";
            dialog.yesButtonText = "Yes";
            dialog.noButtonText = "No";
            dialog.yesButtonHandler = () =>
            {
                File.Delete(MenuGUI.GetMapPath(fileName));
                handler();
                Destroy(this);
            };
            dialog.noButtonHandler = () =>
            {
                Destroy(this);
            };
        }
    }
}