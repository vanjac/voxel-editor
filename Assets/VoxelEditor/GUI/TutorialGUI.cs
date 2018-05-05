using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialPage
{
    public virtual void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject) { }
    public virtual void Update(VoxelArrayEditor voxelArray, GameObject guiGameObject) { }
    public virtual void End(VoxelArrayEditor voxelArray, GameObject guiGameObject) { }
    public abstract string GetText();
    public virtual Tutorials.PageId GetNextButtonTarget()
    {
        return Tutorials.PageId.NONE;
    }
}

public class SimpleTutorialPage : TutorialPage
{
    private readonly string text;
    private Tutorials.PageId next;

    public SimpleTutorialPage(string text, Tutorials.PageId next=Tutorials.PageId.NONE)
    {
        this.text = text;
        this.next = next;
    }

    public override string GetText()
    {
        return text;
    }

    public override Tutorials.PageId GetNextButtonTarget()
    {
        return next;
    }
}

public class TutorialGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;

    private GUIStyle textStyle;

    private static List<Tutorials.PageId> pageStack = new List<Tutorials.PageId>();
    private TutorialPage currentPage = null;

    private void SetPage(Tutorials.PageId pageId)
    {
        var factory = Tutorials.PAGES[(int)pageId];
        TutorialPage newPage = null;
        if (factory != null)
            newPage = factory();
        if (currentPage != null)
            currentPage.End(voxelArray, gameObject);
        if (newPage != null)
            newPage.Start(voxelArray, gameObject);
        currentPage = newPage;
    }

    public void StartTutorial(Tutorials.PageId pageId) {
        pageStack.Clear();
        pageStack.Add(pageId);
        SetPage(pageId);
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
        float h = .15f - propertiesGUI.slide / PropertiesGUI.SLIDE_HIDDEN * .03f;
        return new Rect(height / 2 + propertiesGUI.slide, height * (1 - h),
            width - height / 2 - propertiesGUI.slide, height * h);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void WindowGUI()
    {
        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.wordWrap = true;
            textStyle.alignment = TextAnchor.MiddleLeft;
            textStyle.normal.background = GUI.skin.box.normal.background;
            textStyle.border = GUI.skin.box.border;
            textStyle.padding.top = 0;
            textStyle.padding.bottom = 0;
        }
        if (currentPage == null)
        {
            Destroy(this);
            return;
        }
        currentPage.Update(voxelArray, gameObject);

        GUILayout.BeginHorizontal();
        if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.x))
        {
            pageStack.Clear();
            SetPage(Tutorials.PageId.NONE);
            return;
        }
        if (pageStack.Count > 1 && ActionBarGUI.ActionBarButton(GUIIconSet.instance.close))
        {
            // pop
            pageStack.RemoveAt(pageStack.Count - 1);
            SetPage(pageStack[pageStack.Count - 1]);
        }
        GUILayout.Label(currentPage.GetText(), textStyle, GUILayout.ExpandHeight(true));
        var next = currentPage.GetNextButtonTarget();
        if (next != Tutorials.PageId.NONE && ActionBarGUI.ActionBarButton(GUIIconSet.instance.next))
        {
            // push
            pageStack.Add(next);
            SetPage(next);
        }
        GUILayout.EndHorizontal();
    }
}