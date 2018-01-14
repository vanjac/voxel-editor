using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBarGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;
    public EditorFile editorFile;
    public TouchListener touchListener;

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
    // for overflow menu:
    public Texture worldIcon;
    public Texture doneIcon; // for Entity picker

    protected PropertiesGUI propertiesGUI;

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
        return GUIStyle.none;
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
            labelStyle.normal.background = GUI.skin.box.normal.background;
            labelStyle.border = GUI.skin.box.border;
        }

        GUILayout.BeginHorizontal();

        if (ActionBarButton(closeIcon))
            editorFile.Close();

        SelectionGUI();
        EditGUI();

        GUILayout.FlexibleSpace();

        if (ActionBarButton(playIcon))
            editorFile.Play();

        if (ActionBarButton(overflowIcon))
        {
            gameObject.AddComponent<OverflowMenuGUI>();
        }

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

        if (ActionBarButton(paintIcon))
        {
            PaintGUI paintGUI = gameObject.AddComponent<PaintGUI>();
            paintGUI.title = "Paint Faces";
            paintGUI.voxelArray = voxelArray;
            paintGUI.paint = voxelArray.GetSelectedPaint();
        }

        if (ActionBarButton(createIcon))
        {
            TypePickerGUI picker = gameObject.AddComponent<TypePickerGUI>();
            picker.title = "Create Object";
            picker.items = GameScripts.entityTemplates;
            picker.handler = (PropertiesObjectType type) =>
            {
                Substance substance = (Substance)type.Create();
                voxelArray.substanceToCreate = substance;
                var createGUI = gameObject.AddComponent<CreateSubstanceGUI>();
                createGUI.voxelArray = voxelArray;
            };
        }

        GUILayout.FlexibleSpace();
        int moveCount = 0;
        if (touchListener.currentTouchOperation == TouchListener.TouchOperation.MOVE)
            moveCount = Mathf.Abs(touchListener.movingAxis.moveCount);
        if (moveCount != 0)
            ActionBarLabel(moveCount.ToString());
        else
            ActionBarLabel(SelectionString(voxelArray.selectionBounds.size));
    }

    protected bool ActionBarButton(Texture icon)
    {
        return GUILayout.Button(icon, buttonStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
    }

    protected bool HighlightedActionBarButton(Texture icon)
    {
        return !GUILayout.Toggle(true, icon, buttonStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
    }

    protected void ActionBarLabel(string text)
    {
        if (text.Length == 0)
            return;
        GUILayout.Label(text, labelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
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
