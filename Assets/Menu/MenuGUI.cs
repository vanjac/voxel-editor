﻿using System.Collections.Generic;
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

    private GUIContent[] startOptions;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.HorizCenterRect(safeRect.center.x, safeRect.yMin,
            safeRect.width * .6f, safeRect.height, maxWidth: 1360);

    public override GUIStyle GetStyle() => GUIStyle.none;

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    void Start()
    {
        UpdateWorldList();
        startOptions = new GUIContent[]
        {
            new GUIContent("Tutorial", IconSet.helpLarge),
            new GUIContent("New World", IconSet.newWorldLarge)
        };
    }

    public override void WindowGUI()
    {
        if (worldPaths.Count == 0)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // copied from TemplatePickerGUI
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(900), GUILayout.Height(480));
            GUILayout.Label("Welcome to N-Space\nFollowing the tutorial is recommended!", GUIUtils.LABEL_HORIZ_CENTERED.Value);
            int selection = GUILayout.SelectionGrid(-1, startOptions, 2,
                TemplatePickerGUI.buttonStyle.Value, GUILayout.ExpandHeight(true));
            if (selection == 0)
            {
                TutorialGUI.StartTutorial(Tutorials.INTRO_TUTORIAL, null, null, null);
                HelpGUI.OpenDemoWorld("Tutorial - Introduction", "Templates/indoor");
            }
            else if (selection == 1)
            {
                AskNewWorldTemplate();
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        else
        {
            if (GUIUtils.HighlightedButton(GUIUtils.MenuContent("New World", IconSet.newItem),
                    StyleSet.buttonLarge))
                AskNewWorldTemplate();
            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < worldPaths.Count; i++)
            {
                string path = worldPaths[i];
                string name = worldNames[i];
                bool selected = worldOverflowMenu != null && path == selectedWorldPath;

                GUILayout.BeginHorizontal();
                GUIUtils.BeginHorizontalClipped(GUILayout.ExpandHeight(false));
                if (GUIUtils.HighlightedButton(name, StyleSet.buttonLarge, selected))
                    OpenWorld(path, Scenes.EDITOR);
                GUIUtils.EndHorizontalClipped();
                if (GUIUtils.HighlightedButton(IconSet.overflow, StyleSet.buttonLarge,
                        selected, GUILayout.ExpandWidth(false)))
                    CreateWorldOverflowMenu(path);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
    }

    public static void OpenWorld(string path, string scene)
    {
        SelectedWorld.SelectSavedWorld(path);
        SceneManager.LoadScene(scene);
    }

    private void AskNewWorldTemplate()
    {
        var menu = gameObject.AddComponent<TemplatePickerGUI>();
        menu.handler = (value) =>
        {
            switch (value)
            {
                case 0:
                    AskNewWorldName(indoorTemplate);
                    break;
                case 1:
                    AskNewWorldName(floatingTemplate);
                    break;
            }
        };
    }

    private void AskNewWorldName(TextAsset template)
    {
        TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
        inputDialog.prompt = "Enter new world name...";
        inputDialog.text = "Untitled " + System.DateTime.Now.ToString("yyyy-MM-dd HHmmss");
        inputDialog.handler = (string name) => NewWorld(name, template);
    }

    private void NewWorld(string name, TextAsset template)
    {
        if (!WorldFiles.ValidateName(name, out string errorMessage))
        {
            if (errorMessage != null)
                DialogGUI.ShowMessageDialog(gameObject, errorMessage);
            return;
        }
        string path = WorldFiles.GetNewWorldPath(name);
        try
        {
            using (FileStream fileStream = File.Create(path))
            {
                fileStream.Write(template.bytes, 0, template.bytes.Length);
            }
        }
        catch (System.Exception ex)
        {
            DialogGUI.ShowMessageDialog(gameObject, "Error creating world file");
            Debug.LogError(ex);
            return;
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
            new OverflowMenuGUI.MenuItem("Play", IconSet.play, () => {
                MenuGUI.OpenWorld(path, Scenes.GAME);
            }),
            new OverflowMenuGUI.MenuItem("Rename", IconSet.rename, () => {
                TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
                inputDialog.prompt = "Enter new name for " + name;
                inputDialog.text = name;
                inputDialog.handler = RenameWorld;
            }),
            new OverflowMenuGUI.MenuItem("Copy", IconSet.copy, () => {
                TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
                inputDialog.prompt = "Enter new world name...";
                inputDialog.handler = CopyWorld;
            }),
            new OverflowMenuGUI.MenuItem("Delete", IconSet.delete, () => {
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
            new OverflowMenuGUI.MenuItem("Share", IconSet.share, () => ShareMap.Share(path))
#endif
        };
    }

    private void RenameWorld(string newName)
    {
        if (!WorldFiles.ValidateName(newName, out string errorMessage))
        {
            if (errorMessage != null)
                DialogGUI.ShowMessageDialog(gameObject, errorMessage);
            return;
        }
        string newPath = WorldFiles.GetNewWorldPath(newName);
        File.Move(selectedWorldPath, newPath);
        UpdateWorldList();
    }

    private void CopyWorld(string newName)
    {
        if (!WorldFiles.ValidateName(newName, out string errorMessage))
        {
            if (errorMessage != null)
                DialogGUI.ShowMessageDialog(gameObject, errorMessage);
            return;
        }
        string newPath = WorldFiles.GetNewWorldPath(newName);
        File.Copy(selectedWorldPath, newPath);
        UpdateWorldList();
    }
}
