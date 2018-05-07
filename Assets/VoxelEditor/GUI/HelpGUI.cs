using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            StartTutorial(Tutorials.PageId.INTRO_WELCOME);
        if (GUILayout.Button("Painting"))
            StartTutorial(Tutorials.PageId.PAINT_START);
    }

    private void StartTutorial(Tutorials.PageId page)
    {
        var tutorialGUI = GetComponent<TutorialGUI>();
        if (tutorialGUI == null)
            tutorialGUI = gameObject.AddComponent<TutorialGUI>();
        tutorialGUI.voxelArray = voxelArray;
        tutorialGUI.touchListener = touchListener;
        tutorialGUI.StartTutorial(page);
        Destroy(this);
    }
}