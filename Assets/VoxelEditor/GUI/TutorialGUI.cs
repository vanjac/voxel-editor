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

    private static List<Tutorials.PageId> pageStack = new List<Tutorials.PageId>();
    private TutorialPage currentPage = null;

    private void SetPage(Tutorials.PageId pageId)
    {
        TutorialPage newPage = Tutorials.PAGES[(int)pageId]();
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
        return new Rect(height / 2 + propertiesGUI.slide, height * .88f,
            width - height / 2 - propertiesGUI.slide, height * .12f);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void WindowGUI()
    {
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
        GUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label(currentPage.GetText(), GUILayout.ExpandHeight(true));
        GUILayout.EndHorizontal();
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