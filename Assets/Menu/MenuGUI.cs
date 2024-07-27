using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class MenuGUI : GUIPanel {
    public TextAsset indoorTemplate, floatingTemplate;

    public static readonly System.Lazy<GUIStyle> worldButtonStyle = new System.Lazy<GUIStyle>(() =>
        new GUIStyle(StyleSet.buttonLarge) {
            alignment = TextAnchor.MiddleLeft,
        });

    private List<string> worldPaths = new List<string>();
    private List<string> worldNames = new List<string>();
    private OverflowMenuGUI worldOverflowMenu;
    private string selectedWorldPath;

    private GUIContent[] startOptions;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.HorizCenterRect(safeRect.center.x, safeRect.yMin,
            safeRect.width * .6f, safeRect.height, maxWidth: 1360);

    public override GUIStyle GetStyle() => GUIStyle.none;

    public override void OnEnable() {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    void Start() {
        AssetPack.UnloadUnused(); // no AssetBundle assets in this scene!
        UpdateWorldList();
    }

    public override void WindowGUI() {
#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerMenuGUI();
#else
        if (worldPaths.Count == 0) {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // copied from TemplatePickerGUI
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(900), GUILayout.Height(480));
            GUILayout.Label(StringSet.WelcomeMessage, GUIUtils.LABEL_HORIZ_CENTERED.Value);
            int selection = GUILayout.SelectionGrid(-1, new GUIContent[] {
                new GUIContent(StringSet.StartTutorial, IconSet.helpLarge),
                new GUIContent(StringSet.CreateNewWorld, IconSet.newWorldLarge)
            }, 2, TemplatePickerGUI.buttonStyle.Value, GUILayout.ExpandHeight(true));
            if (selection == 0) {
                TutorialGUI.StartTutorial(Tutorials.INTRO_TUTORIAL, null, null, null);
                HelpGUI.OpenDemoWorld(StringSet.TutorialWorldName(StringSet.TutorialIntro),
                    "Templates/indoor");
            } else if (selection == 1) {
                AskNewWorldTemplate();
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        } else {
            if (GUIUtils.HighlightedButton(
                    GUIUtils.MenuContent(StringSet.CreateNewWorld, IconSet.newItem),
                    StyleSet.buttonLarge)) {
                AskNewWorldTemplate();
            }
            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < worldPaths.Count; i++) {
                string path = worldPaths[i];
                string name = worldNames[i];
                bool selected = worldOverflowMenu != null && path == selectedWorldPath;

                GUILayout.BeginHorizontal();
                if (GUIUtils.HighlightedButton(name, worldButtonStyle.Value, selected,
                        GUILayout.MinWidth(0))) {
                    OpenWorld(path, Scenes.EDITOR);
                }
                if (GUIUtils.HighlightedButton(IconSet.overflow, StyleSet.buttonLarge,
                        selected, GUILayout.ExpandWidth(false))) {
                    CreateWorldOverflowMenu(path);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private void PlayerMenuGUI() {
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(GUIUtils.MenuContent("Select File...", IconSet.openFile),
                StyleSet.buttonLarge)) {
            OpenFilePicker();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }
#endif

    public static void OpenWorld(string path, string scene) {
        SelectedWorld.SelectSavedWorld(path);
        SceneManager.LoadScene(scene);
    }

    private void AskNewWorldTemplate() {
        var menu = gameObject.AddComponent<TemplatePickerGUI>();
        menu.handler = (value) => {
            switch (value) {
                case 0:
                    AskNewWorldName(indoorTemplate);
                    break;
                case 1:
                    AskNewWorldName(floatingTemplate);
                    break;
            }
        };
    }

    private void AskNewWorldName(TextAsset template) {
        TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
        inputDialog.prompt = StringSet.WorldNamePrompt;
        inputDialog.text = StringSet.UntitledWorldName(System.DateTime.Now);
        inputDialog.handler = (string name) => NewWorld(name, template);
    }

    private void NewWorld(string name, TextAsset template) {
        if (!WorldFiles.ValidateName(name, out string errorMessage)) {
            if (errorMessage != null) {
                DialogGUI.ShowMessageDialog(gameObject, errorMessage);
            }
            return;
        }
        string path = WorldFiles.GetNewWorldPath(name);
        try {
            using (FileStream fileStream = File.Create(path)) {
                fileStream.Write(template.bytes, 0, template.bytes.Length);
            }
        } catch (System.Exception ex) {
            DialogGUI.ShowMessageDialog(gameObject, StringSet.ErrorCreatingWorld);
            Debug.LogError(ex);
            return;
        }
        UpdateWorldList();

        OpenWorld(path, Scenes.EDITOR);
    }

    private void UpdateWorldList() {
        WorldFiles.ListWorlds(worldPaths, worldNames);
    }

    private void CreateWorldOverflowMenu(string path) {
        string name = Path.GetFileNameWithoutExtension(path);
        worldOverflowMenu = gameObject.AddComponent<OverflowMenuGUI>();
        selectedWorldPath = path;
        worldOverflowMenu.items = new OverflowMenuGUI.MenuItem[] {
            new OverflowMenuGUI.MenuItem(StringSet.PlayWorld, IconSet.play, () => {
                MenuGUI.OpenWorld(path, Scenes.GAME);
            }),
            new OverflowMenuGUI.MenuItem(StringSet.RenameWorld, IconSet.rename, () => {
                TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
                inputDialog.prompt = StringSet.WorldRenamePrompt(name);
                inputDialog.text = name;
                inputDialog.handler = RenameWorld;
            }),
            new OverflowMenuGUI.MenuItem(StringSet.CopyWorld, IconSet.copy, () => {
                TextInputDialogGUI inputDialog = gameObject.AddComponent<TextInputDialogGUI>();
                inputDialog.prompt = StringSet.WorldNamePrompt;
                inputDialog.handler = CopyWorld;
            }),
            new OverflowMenuGUI.MenuItem(StringSet.DeleteWorld, IconSet.delete, () => {
                DialogGUI dialog = gameObject.AddComponent<DialogGUI>();
                dialog.message = StringSet.WorldDeleteConfirm(name);
                dialog.yesButtonText = StringSet.Yes;
                dialog.noButtonText = StringSet.No;
                dialog.yesButtonHandler = () => {
                    File.Delete(path);
                    File.Delete(WorldFiles.GetThumbnailPath(path));
                    UpdateWorldList();
                };
            }),
#if UNITY_ANDROID || UNITY_IOS
            new OverflowMenuGUI.MenuItem(StringSet.ShareWorld, IconSet.share,
                () => ShareMap.Share(path))
#endif
        };
    }

    private void RenameWorld(string newName) {
        if (!WorldFiles.ValidateName(newName, out string errorMessage)) {
            if (errorMessage != null) {
                DialogGUI.ShowMessageDialog(gameObject, errorMessage);
            }
            return;
        }
        string newPath = WorldFiles.GetNewWorldPath(newName);
        File.Move(selectedWorldPath, newPath);
        var newThumbPath = WorldFiles.GetThumbnailPath(newPath);
        try {
            File.Delete(newThumbPath);
            File.Move(WorldFiles.GetThumbnailPath(selectedWorldPath), newThumbPath);
        } catch (IOException) { }
        UpdateWorldList();
    }

    private void CopyWorld(string newName) {
        if (!WorldFiles.ValidateName(newName, out string errorMessage)) {
            if (errorMessage != null) {
                DialogGUI.ShowMessageDialog(gameObject, errorMessage);
            }
            return;
        }
        string newPath = WorldFiles.GetNewWorldPath(newName);
        File.Copy(selectedWorldPath, newPath);
        var newThumbPath = WorldFiles.GetThumbnailPath(newPath);
        try {
            File.Copy(WorldFiles.GetThumbnailPath(selectedWorldPath), newThumbPath, true);
        } catch (IOException) { }
        UpdateWorldList();
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool OpenFilePicker();

    void LoadWebGLFile() {
        OpenWorld("/tmp/mapsave", Scenes.GAME);
    }
#endif
}
