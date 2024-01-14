using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;
    public TouchListener touchListener;

    private int tab = 0;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            576, safeRect.height * .8f, maxHeight: 1280);

    public void Start()
    {
        title = StringSet.HelpMenuTitle;
    }

    public override void WindowGUI()
    {
        int oldTab = tab;
        tab = GUILayout.SelectionGrid(tab,
            new string[] { StringSet.HelpTutorials, StringSet.HelpDemoWorlds }, 2,
            StyleSet.buttonTab);
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
        if (HelpButton(StringSet.TutorialIntro))
            StartTutorial(Tutorials.INTRO_TUTORIAL, StringSet.TutorialIntro, forceIndoor: true);
        if (HelpButton(StringSet.TutorialPainting))
            StartTutorial(Tutorials.PAINT_TUTORIAL, StringSet.TutorialPainting);
        if (HelpButton(StringSet.TutorialBevels))
            StartTutorial(Tutorials.BEVEL_TUTORIAL, StringSet.TutorialBevels);
        if (HelpButton(StringSet.TutorialSubstances))
            StartTutorial(Tutorials.SUBSTANCE_TUTORIAL,
                StringSet.TutorialSubstances, forceIndoor: true);
        if (HelpButton(StringSet.TutorialObjects))
            StartTutorial(Tutorials.OBJECT_TUTORIAL, StringSet.TutorialObjects);
        if (HelpButton(StringSet.TutorialTips))
        {
            LargeMessageGUI.ShowLargeMessageDialog(gameObject, StringSet.TutorialTipsMessage);
            Destroy(this);
        }
        if (HelpButton(StringSet.TutorialAdvancedGameLogic1))
        {
            StartTutorial(Tutorials.ADVANCED_GAME_LOGIC_TUTORIAL_1);
            OpenDemoWorld(StringSet.TutorialWorldName(StringSet.TutorialAdvancedGameLogic1),
                "Tutorials/advanced_game_logic_1");
        }
        if (HelpButton(StringSet.TutorialAdvancedGameLogic2))
            StartTutorial(Tutorials.ADVANCED_GAME_LOGIC_TUTORIAL_2,
                StringSet.TutorialAdvancedGameLogic2, forceIndoor: true);
    }

    private void DemoWorldsTab()
    {
        DemoWorldButton(StringSet.DemoDoors, "doors");
        DemoWorldButton(StringSet.DemoHovercraft, "hovercraft");
        DemoWorldButton(StringSet.DemoAI, "ai");
        DemoWorldButton(StringSet.DemoPlatforms, "platforms");
        DemoWorldButton(StringSet.DemoShapes, "shapes");
        DemoWorldButton(StringSet.DemoLogic, "logic");
        DemoWorldButton(StringSet.DemoImpossibleHallway, "impossible_hallway");
        DemoWorldButton(StringSet.DemoConveyor, "conveyor");
        DemoWorldButton(StringSet.DemoBallPit, "ball_pit");
    }

    private bool HelpButton(string text) => GUILayout.Button(text, StyleSet.buttonLarge);

    private void DemoWorldButton(string name, string file)
    {
        if (HelpButton(name))
        {
            OpenDemoWorld(StringSet.DemoWorldName(name), "Demos/" + file);
            Destroy(this);
        }
    }

    private void StartTutorial(TutorialPageFactory[] tutorial, string worldName = null,
        bool forceIndoor = false)
    {
        TutorialGUI.StartTutorial(tutorial, gameObject, voxelArray, touchListener);
        if (worldName != null && (voxelArray == null ||
                (voxelArray.type != VoxelArray.WorldType.INDOOR && forceIndoor)))
            OpenDemoWorld(StringSet.TutorialWorldName(worldName), "Templates/indoor");
        Destroy(this);
    }

    public static void OpenDemoWorld(string name, string templateName)
    {
        if (EditorFile.instance != null)
        {
            if (!EditorFile.instance.Save())
                return;
        }

        TextAsset worldAsset = Resources.Load<TextAsset>(templateName);
        string path = WorldFiles.GetNewWorldPath(name);
        for (int i = 2; File.Exists(path); i++) // autonumber
            path = WorldFiles.GetNewWorldPath(name + " " + i);
        SelectedWorld.SelectDemoWorld(worldAsset, path);
        SceneManager.LoadScene(Scenes.EDITOR);
    }
}