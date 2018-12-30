using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGUI : GUIPanel
{
    public TextAsset defaultMap;

    private List<string> mapFiles = new List<string>();
    private OverflowMenuGUI worldOverflowMenu;
    private string selectedWorld;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(safeRect.xMin + safeRect.width * .2f, safeRect.yMin,
            safeRect.width * .6f, safeRect.height);
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

    void Start()
    {
        UpdateMapList();
    }

    public override void WindowGUI()
    {
        if (GUIUtils.HighlightedButton("New", GUIStyleSet.instance.buttonLarge))
        {
            TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
            inputDialog.prompt = "Enter new world name...";
            inputDialog.handler = NewMap;
        }
        scroll = GUILayout.BeginScrollView(scroll);
        foreach (string fileName in mapFiles)
        {
            bool selected = worldOverflowMenu != null && fileName == selectedWorld;
            GUILayout.BeginHorizontal();
            if (GUIUtils.HighlightedButton(fileName, GUIStyleSet.instance.buttonLarge, selected))
                OpenMap(fileName, "editScene");
            if (GUIUtils.HighlightedButton(GUIIconSet.instance.overflow, GUIStyleSet.instance.buttonLarge,
                    selected, GUILayout.ExpandWidth(false)))
                CreateWorldOverflowMenu(fileName);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        if (mapFiles.Count == 0)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            ActionBarGUI.ActionBarLabel("Tap 'New' to create a new world.");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
    }

    public static void OpenMap(string name, string scene)
    {
        SelectedMap selectedMap = SelectedMap.Instance();
        selectedMap.mapName = name;
        SceneManager.LoadScene(scene);
    }

    private void NewMap(string name)
    {
        if (name.Length == 0)
            return;
        string filePath = WorldFiles.GetFilePath(name);
        if (File.Exists(filePath))
        {
            DialogGUI.ShowMessageDialog(gameObject, "A world with that name already exists.");
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

        OpenMap(name, "editScene");
    }

    private void UpdateMapList()
    {
        string[] files = Directory.GetFiles(WorldFiles.GetDirectoryPath());
        mapFiles.Clear();
        foreach (string name in files)
            mapFiles.Add(Path.GetFileNameWithoutExtension(name));
        mapFiles.Sort();
    }

    private void CreateWorldOverflowMenu(string fileName)
    {
        worldOverflowMenu = gameObject.AddComponent<OverflowMenuGUI>();
        selectedWorld = fileName;
        worldOverflowMenu.items = new OverflowMenuGUI.MenuItem[]
        {
            new OverflowMenuGUI.MenuItem("Play", GUIIconSet.instance.play, () => {
                MenuGUI.OpenMap(fileName, "playScene");
            }),
            new OverflowMenuGUI.MenuItem("Rename", GUIIconSet.instance.rename, () => {
                TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
                inputDialog.prompt = "Enter new name for " + fileName;
                inputDialog.handler = RenameMap;
            }),
            new OverflowMenuGUI.MenuItem("Copy", GUIIconSet.instance.copy, () => {
                TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
                inputDialog.prompt = "Enter new world name...";
                inputDialog.handler = CopyMap;
            }),
            new OverflowMenuGUI.MenuItem("Delete", GUIIconSet.instance.delete, () => {
                DialogGUI dialog = gameObject.AddComponent<DialogGUI>();
                dialog.message = "Are you sure you want to delete " + fileName + "?";
                dialog.yesButtonText = "Yes";
                dialog.noButtonText = "No";
                dialog.yesButtonHandler = () =>
                {
                    File.Delete(WorldFiles.GetFilePath(fileName));
                    UpdateMapList();
                };
            }),
#if (UNITY_ANDROID || UNITY_IOS)
            new OverflowMenuGUI.MenuItem("Share", GUIIconSet.instance.share, () => {
                string path = WorldFiles.GetFilePath(fileName);
                ShareMap.Share(path);
            })
#endif
        };
    }

    private void RenameMap(string newName)
    {
        if (newName.Length == 0)
            return;
        string newPath = WorldFiles.GetFilePath(newName);
        if (File.Exists(newPath))
        {
            DialogGUI.ShowMessageDialog(gameObject, "A world with that name already exists.");
            return;
        }
        File.Move(WorldFiles.GetFilePath(selectedWorld), newPath);
        UpdateMapList();
    }

    private void CopyMap(string newName)
    {
        if (newName.Length == 0)
            return;
        string newPath = WorldFiles.GetFilePath(newName);
        if (File.Exists(newPath))
        {
            DialogGUI.ShowMessageDialog(gameObject, "A world with that name already exists.");
            return;
        }
        File.Copy(WorldFiles.GetFilePath(selectedWorld), newPath);
        UpdateMapList();
    }
}
