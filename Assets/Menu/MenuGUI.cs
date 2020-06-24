using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGUI : GUIPanel
{
    public TextAsset indoorTemplate, floatingTemplate;

    private List<string> worldPaths = new List<string>();
    private List<string> worldNames = new List<string>();
    private OverflowMenuGUI worldOverflowMenu;
    private string selectedWorldPath;

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
        UpdateWorldList();
    }

    public override void WindowGUI()
    {
        if (GUIUtils.HighlightedButton("New", GUIStyleSet.instance.buttonLarge))
        {
            AskNewWorldTemplate();
        }
        scroll = GUILayout.BeginScrollView(scroll);
        for (int i = 0; i < worldPaths.Count; i++)
        {
            string path = worldPaths[i];
            string name = worldNames[i];
            bool selected = worldOverflowMenu != null && path == selectedWorldPath;

            GUILayout.BeginHorizontal();
            if (GUIUtils.HighlightedButton(name, GUIStyleSet.instance.buttonLarge, selected))
                OpenWorld(path, Scenes.EDITOR);
            if (GUIUtils.HighlightedButton(GUIIconSet.instance.overflow, GUIStyleSet.instance.buttonLarge,
                    selected, GUILayout.ExpandWidth(false)))
                CreateWorldOverflowMenu(path);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        if (worldPaths.Count == 0)
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

    public static void OpenWorld(string path, string scene)
    {
        SelectedWorld.SelectSavedWorld(path);
        SceneManager.LoadScene(scene);
    }

    private void AskNewWorldTemplate()
    {
        var menu = gameObject.AddComponent<OverflowMenuGUI>();
        menu.items = new OverflowMenuGUI.MenuItem[]
        {
            new OverflowMenuGUI.MenuItem("Indoor", GUIIconSet.instance.indoor, () => AskNewWorldName(indoorTemplate)),
            new OverflowMenuGUI.MenuItem("Floating", GUIIconSet.instance.floating, () => AskNewWorldName(floatingTemplate))
        };
    }

    private void AskNewWorldName(TextAsset template)
    {
        TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
        inputDialog.prompt = "Enter new world name...";
        inputDialog.handler = (string name) => NewWorld(name, template);
    }

    private void NewWorld(string name, TextAsset template)
    {
        if (name.Length == 0)
            return;
        string path = WorldFiles.GetNewWorldPath(name);
        if (File.Exists(path))
        {
            DialogGUI.ShowMessageDialog(gameObject, "A world with that name already exists.");
            return;
        }
        using (FileStream fileStream = File.Create(path))
        {
            fileStream.Write(template.bytes, 0, template.bytes.Length);
        }
        UpdateWorldList();

        OpenWorld(path, Scenes.EDITOR);
    }

    private void UpdateWorldList()
    {
        WorldFiles.ListWorlds(worldPaths, worldNames);
    }

    private void CreateWorldOverflowMenu(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        worldOverflowMenu = gameObject.AddComponent<OverflowMenuGUI>();
        selectedWorldPath = path;
        worldOverflowMenu.items = new OverflowMenuGUI.MenuItem[]
        {
            new OverflowMenuGUI.MenuItem("Play", GUIIconSet.instance.play, () => {
                MenuGUI.OpenWorld(path, Scenes.GAME);
            }),
            new OverflowMenuGUI.MenuItem("Rename", GUIIconSet.instance.rename, () => {
                TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
                inputDialog.prompt = "Enter new name for " + name;
                inputDialog.handler = RenameWorld;
            }),
            new OverflowMenuGUI.MenuItem("Copy", GUIIconSet.instance.copy, () => {
                TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
                inputDialog.prompt = "Enter new world name...";
                inputDialog.handler = CopyWorld;
            }),
            new OverflowMenuGUI.MenuItem("Delete", GUIIconSet.instance.delete, () => {
                DialogGUI dialog = gameObject.AddComponent<DialogGUI>();
                dialog.message = "Are you sure you want to delete " + name + "?";
                dialog.yesButtonText = "Yes";
                dialog.noButtonText = "No";
                dialog.yesButtonHandler = () =>
                {
                    File.Delete(path);
                    UpdateWorldList();
                };
            }),
#if (UNITY_ANDROID || UNITY_IOS)
            new OverflowMenuGUI.MenuItem("Share", GUIIconSet.instance.share,
                () => ShareMap.Share(path))
#endif
        };
    }

    private void RenameWorld(string newName)
    {
        if (newName.Length == 0)
            return;
        string newPath = WorldFiles.GetNewWorldPath(newName);
        if (File.Exists(newPath))
        {
            DialogGUI.ShowMessageDialog(gameObject, "A world with that name already exists.");
            return;
        }
        File.Move(selectedWorldPath, newPath);
        UpdateWorldList();
    }

    private void CopyWorld(string newName)
    {
        if (newName.Length == 0)
            return;
        string newPath = WorldFiles.GetNewWorldPath(newName);
        if (File.Exists(newPath))
        {
            DialogGUI.ShowMessageDialog(gameObject, "A world with that name already exists.");
            return;
        }
        File.Copy(selectedWorldPath, newPath);
        UpdateWorldList();
    }
}
