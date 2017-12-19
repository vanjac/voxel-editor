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

        GUILayout.FlexibleSpace();

        Vector3 selectionSize = voxelArray.selectionBounds.size;
        if (selectionSize != Vector3.zero)
        {
            GUILayout.Label(selectionSize.ToString());
        }
        else
        {
            GUILayout.Label("");
        }

        GUILayout.EndHorizontal();
    }
}
