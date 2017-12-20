using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBarGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;
    public EditorFile editorFile;

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    public override Rect GetRect(float width, float height)
    {
        return new Rect(height * .5f, 0, width - height * .5f, 0);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Create", GUILayout.ExpandWidth(false)))
        {
            voxelArray.SubstanceTest();
        }

        if (voxelArray.selectMode != VoxelArrayEditor.SelectMode.NONE)
        {
            if (GUILayout.Button("+ Select", GUILayout.ExpandWidth(false)))
            {
                voxelArray.StoreSelection();
            }
        }
        else if (voxelArray.SomethingIsSelected())
        {
            if (GUILayout.Button("- Select", GUILayout.ExpandWidth(false)))
            {
                voxelArray.ClearStoredSelection();
                voxelArray.ClearSelection();
            }
        }

        if (GUILayout.Button("Play", GUILayout.ExpandWidth(false)))
            editorFile.Play();

        if (GUILayout.Button("Close", GUILayout.ExpandWidth(false)))
            editorFile.Close();

        GUIStyle rightAlign = new GUIStyle(GUI.skin.label);
        rightAlign.alignment = TextAnchor.UpperRight;
        GUILayout.Label(SelectionString(voxelArray.selectionBounds.size), rightAlign);

        GUILayout.EndHorizontal();
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
