using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialAction
{
    NONE, NEXT, BACK
}

public abstract class TutorialPage
{
    public virtual void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject,
        TouchListener touchListener)
    { }
    public virtual TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
        TouchListener touchListener)
    {
        return TutorialAction.NONE;
    }
    public virtual void End(VoxelArrayEditor voxelArray, GameObject guiGameObject,
        TouchListener touchListener)
    { }
    public abstract string GetText();
    public virtual bool ShowNextButton()
    {
        return false;
    }
    public virtual string GetHighlightID()
    {
        return "";
    }
}

public delegate TutorialPage TutorialPageFactory();

public class SimpleTutorialPage : TutorialPage
{
    private readonly string text;
    private readonly string highlight;

    public SimpleTutorialPage(string text, string highlight = "")
    {
        this.text = text;
        this.highlight = highlight;
    }

    public override string GetText()
    {
        return text;
    }

    public override bool ShowNextButton()
    {
        return true;
    }

    public override string GetHighlightID()
    {
        return highlight;
    }
}

public class FullScreenTutorialPage : SimpleTutorialPage
{
    private Texture image;
    private float backgroundWidth, backgroundHeight;

    public FullScreenTutorialPage(string text, string imageResourceName,
        float width, float height) : base(text)
    {
        image = Resources.Load<Texture>(imageResourceName);
        backgroundWidth = width;
        backgroundHeight = height;
    }

    public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
    {
        var fade = guiGameObject.AddComponent<FadeGUI>();
        fade.background = image;
        fade.backgroundWidth = backgroundWidth;
        fade.backgroundHeight = backgroundHeight;
    }

    public override void End(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
    {
        GameObject.Destroy(guiGameObject.GetComponent<FadeGUI>());
    }
}

public class TutorialGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;
    public TouchListener touchListener;

    private static readonly Lazy<GUIStyle> buttonStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.GetStyle("button_large"));
        style.fixedHeight = 0;
        return style;
    });
    private static readonly Lazy<GUIStyle> textStyle = new Lazy<GUIStyle>(() =>
    {
        // keep original background because it's more opaque than Box
        var style = new GUIStyle(GUI.skin.GetStyle("button_large"));
        style.wordWrap = true;
        style.alignment = TextAnchor.MiddleLeft;
        style.fixedHeight = 0;
        return style;
    });

    private static TutorialPageFactory[] currentTutorial;
    private static int pageI;
    private static bool resetPageFlag = false;
    private TutorialPage currentPage = null;
    private static string highlightID;
    private float successTime; // for success animation

    public static void StartTutorial(TutorialPageFactory[] tutorial,
        GameObject guiGameObject, VoxelArrayEditor voxelArray, TouchListener touchListener)
    {
        currentTutorial = tutorial;

        // voxelArray is null if opening a tutorial from menuScene
        if (voxelArray != null)
        {
            var tutorialGUI = guiGameObject.GetComponent<TutorialGUI>();
            if (tutorialGUI == null)
                tutorialGUI = guiGameObject.AddComponent<TutorialGUI>();
            tutorialGUI.voxelArray = voxelArray;
            tutorialGUI.touchListener = touchListener;
            tutorialGUI.SetPage(0);
        }
        else
        {
            pageI = 0;
            // create the page when editScene is opened
            resetPageFlag = true;
        }
    }

    public static void TutorialHighlight(string id)
    {
        if (id != highlightID)
            return;
        float x = 0.5f + Time.time % 1.0f / 2;
        GUI.backgroundColor = new Color(1.0f, x, x, 1.0f);
    }

    public static void ClearHighlight()
    {
        GUI.backgroundColor = Color.white;
    }

    private void SetPage(int i)
    {
        pageI = i;
        TutorialPage newPage = null;
        if (currentTutorial != null && pageI >= 0 && pageI < currentTutorial.Length)
            newPage = currentTutorial[pageI]();
        if (currentPage != null)
            currentPage.End(voxelArray, gameObject, touchListener);
        if (newPage != null)
            newPage.Start(voxelArray, gameObject, touchListener);
        currentPage = newPage;
    }

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;

        base.OnEnable();
    }

    private float height;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        float minHeight = GUI.skin.GetStyle("button_large").fixedHeight;
        Rect leftPanelRect = GUIPanel.leftPanel.panelRect;
        this.height = minHeight * (1.25f - (leftPanelRect.xMin - safeRect.xMin) / PropertiesGUI.SLIDE_HIDDEN * .25f);
        return new Rect(leftPanelRect.xMax, safeRect.yMax - this.height,
            safeRect.xMax - leftPanelRect.xMax, this.height);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public void Start()
    {
        SetPage(pageI);
    }

    public override void WindowGUI()
    {
        if (resetPageFlag)
        {
            SetPage(pageI);
            resetPageFlag = false;
        }
        if (currentPage == null)
        {
            highlightID = "";
            Destroy(this);
            return;
        }
        BringToFront();
        highlightID = currentPage.GetHighlightID();
        var action = currentPage.Update(voxelArray, gameObject, touchListener);
        if (action == TutorialAction.NEXT)
            successTime = Time.time;

        if (Time.time - successTime < 1.0)
            GUI.backgroundColor = new Color(Time.time - successTime, 1.0f, Time.time - successTime);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(GUIIconSet.instance.x, buttonStyle.Value,
                GUILayout.ExpandWidth(false), GUILayout.Height(height)))
        {
            SetPage(-1);
            GUI.backgroundColor = Color.white;
            return;
        }
        if (pageI > 0 && GUILayout.Button(GUIIconSet.instance.close, buttonStyle.Value,
                GUILayout.ExpandWidth(false), GUILayout.Height(height)))
            action = TutorialAction.BACK;
        GUILayout.Label(currentPage.GetText(), textStyle.Value, GUILayout.Height(height));
        if (currentPage.ShowNextButton())
        {
            if (pageI == currentTutorial.Length - 1)
            {
                if (GUIUtils.HighlightedButton(GUIIconSet.instance.done, buttonStyle.Value, true,
                        GUILayout.ExpandWidth(false), GUILayout.Height(height)))
                    action = TutorialAction.NEXT;
            }
            else
            {
                if (GUIUtils.HighlightedButton(GUIIconSet.instance.next, buttonStyle.Value, true,
                        GUILayout.ExpandWidth(false), GUILayout.Height(height)))
                    action = TutorialAction.NEXT;
            }
        }
        GUILayout.EndHorizontal();

        switch (action)
        {
            case TutorialAction.BACK:
                SetPage(pageI - 1);
                break;
            case TutorialAction.NEXT:
                SetPage(pageI + 1);
                break;
            case TutorialAction.NONE:
                break;
        }

        GUI.backgroundColor = Color.white;
    }
}