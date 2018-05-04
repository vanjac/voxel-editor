using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TutorialPage
{
    public readonly string text;
    public readonly Tutorials.PageId next;

    public TutorialPage(string text, Tutorials.PageId next=Tutorials.PageId.NONE)
    {
        this.text = text;
        this.next = next;
    }
}

public class TutorialGUI : GUIPanel
{
    private static List<Tutorials.PageId> pageStack = new List<Tutorials.PageId>();

    public static void StartTutorial(Tutorials.PageId page) {
        pageStack.Clear();
        pageStack.Add(page);
    }

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        propertiesGUI = GetComponent<PropertiesGUI>();

        base.OnEnable();
    }

    private PropertiesGUI propertiesGUI;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(height / 2 + propertiesGUI.slide, height * .88f,
            width - height / 2 - propertiesGUI.slide, height * .12f);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void WindowGUI()
    {
        if (pageStack.Count == 0)
        {
            Destroy(this);
            return;
        }

        var page = Tutorials.PAGES[(int)(pageStack[pageStack.Count - 1])];

        GUILayout.BeginHorizontal();
        if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.x))
        {
            pageStack.Clear();
        }
        if (pageStack.Count > 1 && ActionBarGUI.ActionBarButton(GUIIconSet.instance.close))
        {
            pageStack.RemoveAt(pageStack.Count - 1);
        }
        GUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label(page.text, GUILayout.ExpandHeight(true));
        GUILayout.EndHorizontal();
        if (page.next != Tutorials.PageId.NONE && ActionBarGUI.ActionBarButton(GUIIconSet.instance.next))
        {
            pageStack.Add(page.next);
        }
        GUILayout.EndHorizontal();
    }
}