using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpGUI : GUIPanel
{
    private static readonly string[] DEMO_WORLD_NAMES = new string[]
    { "Logic", "Conveyor", "Ball Pit", "Ball Launcher", "Impossible Hallway", "Platform Game" };
    private static readonly string[] DEMO_WORLD_FILES = new string[]
    { "logic", "conveyor", "ball_pit", "launcher", "impossible_hallway", "platforms" };

    public VoxelArrayEditor voxelArray;
    public TouchListener touchListener;

    private int tab = 0;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.HorizCenterRect(safeRect.center.x,
            safeRect.yMin + safeRect.height * .1f, 576, 0);
    }

    public void Start()
    {
        title = "Help";
    }

    public override void WindowGUI()
    {
        tab = GUILayout.SelectionGrid(tab,
            new string[] { "Tutorials", "Demo Worlds" }, 2, GUIStyleSet.instance.buttonTab);
        if (tab == 0)
            TutorialsTab();
        if (tab == 1)
            DemoWorldsTab();
    }

    private void TutorialsTab()
    {
        if (GUILayout.Button("Introduction"))
            StartTutorial(Tutorials.INTRO_TUTORIAL, "Introduction");
        if (GUILayout.Button("Painting"))
            StartTutorial(Tutorials.PAINT_TUTORIAL, "Painting");
        if (GUILayout.Button("Bevels"))
            StartTutorial(Tutorials.BEVEL_TUTORIAL, "Bevels");
        if (GUILayout.Button("Substances"))
            StartTutorial(Tutorials.SUBSTANCE_TUTORIAL, "Substances");
        if (GUILayout.Button("Objects"))
            StartTutorial(Tutorials.OBJECT_TUTORIAL, "Objects");
        if (GUILayout.Button("Tips and Shortcuts"))
        {
            LargeMessageGUI.ShowLargeMessageDialog(gameObject, Tutorials.TIPS_AND_SHORTCUTS_TUTORIAL);
            Destroy(this);
        }
        if (GUILayout.Button("Advanced game logic 1"))
        {
            StartTutorial(Tutorials.ADVANCED_GAME_LOGIC_TUTORIAL_1);
            OpenDemoWorld("Tutorial - Advanced game logic 1", "Tutorials/advanced_game_logic_1");
        }
        if (GUILayout.Button("Advanced game logic 2"))
            StartTutorial(Tutorials.ADVANCED_GAME_LOGIC_TUTORIAL_2, "Advanced game logic 2");
    }

    private void DemoWorldsTab()
    {
        for (int i = 0; i < DEMO_WORLD_NAMES.Length; i++)
        {
            if (GUILayout.Button(DEMO_WORLD_NAMES[i]))
                OpenDemoWorld("Demo - " + DEMO_WORLD_NAMES[i],
                    "Demos/" + DEMO_WORLD_FILES[i]);
        }
    }

    private void StartTutorial(TutorialPageFactory[] tutorial, string worldName = null)
    {
        TutorialGUI.StartTutorial(tutorial, gameObject, voxelArray, touchListener);
        if (worldName != null && voxelArray == null)
            OpenDemoWorld("Tutorial - " + worldName, "Templates/indoor");
        Destroy(this);
    }

    private void OpenDemoWorld(string name, string templateName)
    {
        if (voxelArray != null)
            EditorFile.instance.Save();

        TextAsset worldAsset = Resources.Load<TextAsset>(templateName);
        string path = WorldFiles.GetNewWorldPath(name);
        for (int i = 2; File.Exists(path); i++) // autonumber
            path = WorldFiles.GetNewWorldPath(name + " " + i);
        SelectedWorld.SelectDemoWorld(worldAsset, path);
        SceneManager.LoadScene(Scenes.EDITOR);
        Destroy(this);
    }
}