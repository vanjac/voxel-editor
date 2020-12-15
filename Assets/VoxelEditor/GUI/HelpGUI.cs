using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpGUI : GUIPanel
{
    private static readonly string[] DEMO_WORLD_NAMES = new string[]
    { "Ball Launcher", "Character AI", "Platform Game", "Shapes", "Logic", "Impossible Hallway", "Conveyor", "Ball Pit" };
    private static readonly string[] DEMO_WORLD_FILES = new string[]
    { "launcher", "ai", "platforms", "shapes", "logic", "impossible_hallway", "conveyor", "ball_pit" };

    public VoxelArrayEditor voxelArray;
    public TouchListener touchListener;

    private int tab = 0;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            576, safeRect.height * .8f, maxHeight: 1280);
    }

    public void Start()
    {
        title = "Help";
    }

    public override void WindowGUI()
    {
        int oldTab = tab;
        tab = GUILayout.SelectionGrid(tab,
            new string[] { "Tutorials", "Demo Worlds" }, 2, GUIStyleSet.instance.buttonTab);
        if (oldTab != tab)
        {
            scroll = Vector2.zero;
            scrollVelocity = Vector2.zero;
        }
        scroll = GUILayout.BeginScrollView(scroll);
        if (tab == 0)
            TutorialsTab();
        if (tab == 1)
            DemoWorldsTab();
        GUILayout.EndScrollView();
    }

    private void TutorialsTab()
    {
        if (HelpButton("Introduction"))
            StartTutorial(Tutorials.INTRO_TUTORIAL, "Introduction", forceIndoor: true);
        if (HelpButton("Painting"))
            StartTutorial(Tutorials.PAINT_TUTORIAL, "Painting");
        if (HelpButton("Bevels"))
            StartTutorial(Tutorials.BEVEL_TUTORIAL, "Bevels");
        if (HelpButton("Substances"))
            StartTutorial(Tutorials.SUBSTANCE_TUTORIAL, "Substances", forceIndoor: true);
        if (HelpButton("Objects"))
            StartTutorial(Tutorials.OBJECT_TUTORIAL, "Objects");
        if (HelpButton("Tips and Shortcuts"))
        {
            LargeMessageGUI.ShowLargeMessageDialog(gameObject, Tutorials.TIPS_AND_SHORTCUTS_TUTORIAL);
            Destroy(this);
        }
        if (HelpButton("Advanced Game Logic 1"))
        {
            StartTutorial(Tutorials.ADVANCED_GAME_LOGIC_TUTORIAL_1);
            OpenDemoWorld("Tutorial - Advanced game logic 1", "Tutorials/advanced_game_logic_1");
        }
        if (HelpButton("Advanced Game Logic 2"))
            StartTutorial(Tutorials.ADVANCED_GAME_LOGIC_TUTORIAL_2, "Advanced game logic 2", forceIndoor: true);
    }

    private void DemoWorldsTab()
    {
        for (int i = 0; i < DEMO_WORLD_NAMES.Length; i++)
        {
            if (HelpButton(DEMO_WORLD_NAMES[i]))
            {
                OpenDemoWorld("Demo - " + DEMO_WORLD_NAMES[i],
                    "Demos/" + DEMO_WORLD_FILES[i]);
                Destroy(this);
            }
        }
    }

    private bool HelpButton(string text)
    {
        return GUILayout.Button(text, GUIStyleSet.instance.buttonLarge);
    }

    private void StartTutorial(TutorialPageFactory[] tutorial, string worldName = null,
        bool forceIndoor = false)
    {
        TutorialGUI.StartTutorial(tutorial, gameObject, voxelArray, touchListener);
        if (worldName != null && (voxelArray == null || 
                (voxelArray.type != VoxelArray.WorldType.INDOOR && forceIndoor)))
            OpenDemoWorld("Tutorial - " + worldName, "Templates/indoor");
        Destroy(this);
    }

    public static void OpenDemoWorld(string name, string templateName)
    {
        if (EditorFile.instance != null)
            EditorFile.instance.Save();

        TextAsset worldAsset = Resources.Load<TextAsset>(templateName);
        string path = WorldFiles.GetNewWorldPath(name);
        for (int i = 2; File.Exists(path); i++) // autonumber
            path = WorldFiles.GetNewWorldPath(name + " " + i);
        SelectedWorld.SelectDemoWorld(worldAsset, path);
        SceneManager.LoadScene(Scenes.EDITOR);
    }
}