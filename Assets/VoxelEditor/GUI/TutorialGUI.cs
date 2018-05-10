﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialAction
{
    NONE, NEXT, BACK, END
}

public abstract class TutorialPage
{
    public virtual void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject,
        TouchListener touchListener) { }
    public virtual TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
        TouchListener touchListener)
    {
        return TutorialAction.NONE;
    }
    public virtual void End(VoxelArrayEditor voxelArray, GameObject guiGameObject,
        TouchListener touchListener) { }
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

    public SimpleTutorialPage(string text, string highlight="")
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
    private float scale;

    public FullScreenTutorialPage(string text, string imageResourceName,
        float scale = 1.0f) : base(text)
    {
        image = Resources.Load<Texture>(imageResourceName);
        this.scale = scale;
    }

    public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
    {
        var fade = guiGameObject.AddComponent<FadeGUI>();
        fade.background = image;
        fade.backgroundScale = scale;
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
    private TutorialPage currentPage = null;
    private static string highlightID;

    public static void TutorialHighlight(string id)
    {
        if (id == highlightID && Time.time % 0.6f < 0.3f) // blink
            GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
    }

    public static void ClearHighlight()
    {
        GUI.backgroundColor = Color.white;
    }

    private void SetPage(int i)
    {
        pageI = i;
        TutorialPage newPage = null;
        if(currentTutorial != null && pageI >= 0 && pageI < currentTutorial.Length)
            newPage = currentTutorial[pageI]();
        if (currentPage != null)
            currentPage.End(voxelArray, gameObject, touchListener);
        if (newPage != null)
            newPage.Start(voxelArray, gameObject, touchListener);
        currentPage = newPage;
    }

    public void StartTutorial(TutorialPageFactory[] tutorial)
    {
        currentTutorial = tutorial;
        SetPage(0);
    }

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        propertiesGUI = GetComponent<PropertiesGUI>();

        base.OnEnable();
    }

    private PropertiesGUI propertiesGUI;
    private float height;

    public override Rect GetRect(float width, float height)
    {
        float minHeight = GUI.skin.GetStyle("button_large").fixedHeight;
        this.height = minHeight * (1.25f - propertiesGUI.slide / PropertiesGUI.SLIDE_HIDDEN * .25f);
        return new Rect(height / 2 + propertiesGUI.slide, height - this.height,
            width - height / 2 - propertiesGUI.slide, this.height);
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
        if (currentPage == null)
        {
            Destroy(this);
            return;
        }
        BringToFront();
        highlightID = currentPage.GetHighlightID();
        var action = currentPage.Update(voxelArray, gameObject, touchListener);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(GUIIconSet.instance.x, buttonStyle.Value,
                GUILayout.ExpandWidth(false), GUILayout.Height(height)))
            action = TutorialAction.END;
        if (pageI > 0 && GUILayout.Button(GUIIconSet.instance.close, buttonStyle.Value,
                GUILayout.ExpandWidth(false), GUILayout.Height(height)))
            action = TutorialAction.BACK;
        GUILayout.Label(currentPage.GetText(), textStyle.Value, GUILayout.Height(height));
        if (currentPage.ShowNextButton())
        {
            if (pageI == currentTutorial.Length - 1)
            {
                if (GUIUtils.HighlightedButton(GUIIconSet.instance.done, buttonStyle.Value,
                        GUILayout.ExpandWidth(false), GUILayout.Height(height)))
                    action = TutorialAction.END;
            }
            else
            {
                if (GUIUtils.HighlightedButton(GUIIconSet.instance.next, buttonStyle.Value,
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
            case TutorialAction.END:
                SetPage(-1);
                return;
            case TutorialAction.NONE:
                break;
        }
    }
}