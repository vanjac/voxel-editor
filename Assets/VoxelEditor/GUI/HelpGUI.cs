using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;
    public TouchListener touchListener;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .35f, height * .25f, width * .3f, 0);
    }

    public void Start()
    {
        title = "Tutorials:";
    }

    public override void WindowGUI()
    {
        if (GUILayout.Button("Introduction"))
            StartTutorial(Tutorials.INTRO_TUTORIAL);
        if (GUILayout.Button("Painting"))
            StartTutorial(Tutorials.PAINT_TUTORIAL);
        if (GUILayout.Button("Substances"))
            StartTutorial(Tutorials.SUBSTANCE_TUTORIAL);
        if (GUILayout.Button("Tips and Shortcuts"))
        {
            LargeMessageGUI.ShowLargeMessageDialog(gameObject, Tutorials.TIPS_AND_SHORTCUTS_TUTORIAL);
            Destroy(this);
        }
        if (GUILayout.Button("Advanced game logic 1"))
        {
            StartTutorialWithTemplate(Tutorials.ADVANCED_GAME_LOGIC_TUTORIAL_1,
                "Advanced game logic 1", "Tutorials/advanced_game_logic_1");
        }
    }

    private void StartTutorial(TutorialPageFactory[] tutorial)
    {
        var tutorialGUI = GetComponent<TutorialGUI>();
        if (tutorialGUI == null)
            tutorialGUI = gameObject.AddComponent<TutorialGUI>();
        tutorialGUI.voxelArray = voxelArray;
        tutorialGUI.touchListener = touchListener;
        tutorialGUI.StartTutorial(tutorial);
        Destroy(this);
    }

    private void StartTutorialWithTemplate(TutorialPageFactory[] tutorial, string mapName, string templateName)
    {
        StartTutorial(tutorial);
        if (SelectedMap.Instance().mapName != mapName)
        {
            // create and load the file
            string filePath = WorldFiles.GetFilePath(mapName);
            if (!File.Exists(filePath))
            {
                TextAsset mapText = Resources.Load<TextAsset>(templateName);
                using (FileStream fileStream = File.Create(filePath))
                {
                    using (var sw = new StreamWriter(fileStream))
                    {
                        sw.Write(mapText.text);
                        sw.Flush();
                    }
                }
            }
            voxelArray.GetComponent<EditorFile>().Save();
            SelectedMap.Instance().mapName = mapName;
            SceneManager.LoadScene("editScene");
        }
    }
}