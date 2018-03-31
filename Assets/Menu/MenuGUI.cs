using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGUI : GUIPanel
{
    public TextAsset defaultMap;

    private List<string> mapFiles = new List<string>();

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
        if (GUILayout.Button("New..."))
        {
            TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
            inputDialog.prompt = "Enter new map name...";
            inputDialog.handler = NewMap;
        }
        scroll = GUILayout.BeginScrollView(scroll);
        foreach (string fileName in mapFiles)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(fileName))
                OpenMap(fileName, "editScene");
            if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
            {
                FileDropdownGUI dropdown = gameObject.AddComponent<FileDropdownGUI>();
                dropdown.fileName = fileName;
                if (Input.touchCount > 0)
                {
                    dropdown.location = Input.GetTouch(0).position;
                    dropdown.location.y = Screen.height - dropdown.location.y;
                    dropdown.location /= scaleFactor;
                    dropdown.location -= new Vector2(30, 30);
                }
                dropdown.handler = () =>
                {
                    UpdateMapList();
                };
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    public static void OpenMap(string name, string scene)
    {
        GameObject selectedMapObject = GameObject.Find("SelectedMap");
        if (selectedMapObject == null)
        {
            selectedMapObject = new GameObject("SelectedMap");
            selectedMapObject.AddComponent<SelectedMap>();
        }
        SelectedMap selectedMap = selectedMapObject.GetComponent<SelectedMap>();
        selectedMap.mapName = name;
        selectedMap.returnFromPlayScene = (scene == "playScene") ? "menuScene" : "editScene";
        SceneManager.LoadScene(scene);
    }

    private void NewMap(string name)
    {
        if (name.Length == 0)
            return;
        string filePath = GetMapPath(name);
        if (File.Exists(filePath))
        {
            DialogGUI.ShowMessageDialog(gameObject, "A map with that name already exists.");
            return;
        }
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
        if (GUILayout.Button("Play"))
        {
            MenuGUI.OpenMap(fileName, "playScene");
        }
        if (GUILayout.Button("Rename"))
        {
            TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
            inputDialog.prompt = "Enter new name for " + fileName;
            inputDialog.handler = RenameMap;
        }
        if (GUILayout.Button("Copy"))
        {
            TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
            inputDialog.prompt = "Enter new map name...";
            inputDialog.handler = CopyMap;
        }
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

    private void RenameMap(string newName)
    {
        if (newName.Length == 0)
            return;
        string newPath = MenuGUI.GetMapPath(newName);
        if (File.Exists(newPath))
        {
            DialogGUI.ShowMessageDialog(gameObject, "A map with that name already exists.");
            return;
        }
        File.Move(MenuGUI.GetMapPath(fileName), newPath);
        handler();
        Destroy(this);
    }

    private void CopyMap(string newName)
    {
        if (newName.Length == 0)
            return;
        string newPath = MenuGUI.GetMapPath(newName);
        if (File.Exists(newPath))
        {
            DialogGUI.ShowMessageDialog(gameObject, "A map with that name already exists.");
            return;
        }
        File.Copy(MenuGUI.GetMapPath(fileName), newPath);
        handler();
        Destroy(this);
    }
}