using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBarGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;
    public EditorFile editorFile;

    private static bool guiInit = false;
    protected static GUIStyle buttonStyle;
    protected static GUIStyle labelStyle;

    public Texture closeIcon;
    public Texture createIcon;
    public Texture applySelectionIcon;
    public Texture clearSelectionIcon;
    public Texture paintIcon;
    public Texture playIcon;
    public Texture overflowIcon;
    public Texture doneIcon; // for Entity picker

    private PropertiesGUI propertiesGUI;

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        propertiesGUI = GetComponent<PropertiesGUI>();

        base.OnEnable();
    }

    public override Rect GetRect(float width, float height)
    {
        return new Rect(height/2 + propertiesGUI.slide, 0,
            width - height/2 - propertiesGUI.slide, height * .12f);
    }

    public override GUIStyle GetStyle()
    {
        return new GUIStyle();
    }

    public override void WindowGUI()
    {
        if (!guiInit)
        {
            guiInit = true;
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding = new RectOffset(40, 40, 16, 16);
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
        }

        GUILayout.BeginHorizontal();

        if (ActionBarButton(closeIcon))
            editorFile.Close();

        SelectionGUI();
        EditGUI();

        GUILayout.FlexibleSpace();

        if (ActionBarButton(playIcon))
            editorFile.Play();

        if (ActionBarButton(overflowIcon)) { }

        GUILayout.EndHorizontal();
    }

    protected void SelectionGUI()
    {
        if (voxelArray.SomethingIsAddSelected())
        {
            if (ActionBarButton(applySelectionIcon))
                voxelArray.StoreSelection();
        }

        if(voxelArray.SomethingIsStoredSelected())
        {
            if (ActionBarButton(clearSelectionIcon))
            {
                voxelArray.ClearStoredSelection();
                voxelArray.ClearSelection();
            }
        }
    }

    protected void EditGUI()
    {
        if (!voxelArray.SomethingIsSelected())
            return;
        if (ActionBarButton(paintIcon)) { }

        if (ActionBarButton(createIcon))
        {
            voxelArray.SubstanceTest();
        }

        GUILayout.Label(SelectionString(voxelArray.selectionBounds.size),
            labelStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }

    protected bool ActionBarButton(Texture icon)
    {
        return GUILayout.Button(icon, buttonStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
    }

    protected bool HighlightedActionBarButton(Texture icon)
    {
        return !GUILayout.Toggle(true, icon, buttonStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
    }

    private string SelectionString(Vector3 selectionSize)
    {
        if (selectionSize == Vector3.zero)
            return "";
        else if (selectionSize.x == 0)
            return Mathf.RoundToInt(selectionSize.y)
                + "x" + Mathf.RoundToInt(selectionSize.z);
        else if (selectionSize.y == 0)
            return Mathf.RoundToInt(selectionSize.x)
                + "x" + Mathf.RoundToInt(selectionSize.z);
        else if (selectionSize.z == 0)
            return Mathf.RoundToInt(selectionSize.x)
                + "x" + Mathf.RoundToInt(selectionSize.y);
        else return Mathf.RoundToInt(selectionSize.x)
                + "x" + Mathf.RoundToInt(selectionSize.y)
                + "x" + Mathf.RoundToInt(selectionSize.z);
    }
}
