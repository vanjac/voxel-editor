using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBarGUI : GUIPanel
{
    public VoxelArray voxelArray;
    public EditorFile editorFile;

    public override void OnGUI()
    {
        base.OnGUI();

        Rect windowRect = new Rect(targetHeight * .55f, 0, scaledScreenWidth - targetHeight * .55f, targetHeight);
        panelRect = new Rect(windowRect.xMin, windowRect.yMin, windowRect.width, GUI.skin.font.fontSize * 2);
        GUILayout.BeginArea(windowRect);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Create", GUILayout.ExpandWidth(false)))
        {
            voxelArray.SubstanceTest();
        }

        if (voxelArray.selectMode != VoxelArray.SelectMode.NONE)
        {
            if (GUILayout.Button("Apply Selection", GUILayout.ExpandWidth(false)))
            {
                voxelArray.StoreSelection();
            }
        }
        else if (voxelArray.SomethingIsSelected())
        {
            if (GUILayout.Button("Clear Selection", GUILayout.ExpandWidth(false)))
            {
                voxelArray.ClearStoredSelection();
                voxelArray.ClearSelection();
            }
        }

        if (GUILayout.Button("Play", GUILayout.ExpandWidth(false)))
            editorFile.Play();

        if (GUILayout.Button("Close", GUILayout.ExpandWidth(false)))
            editorFile.Close();

        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        Vector3 selectionSize = voxelArray.selectionBounds.size;
        if (selectionSize != Vector3.zero)
        {
            GUI.skin.label.alignment = TextAnchor.LowerRight;
            GUILayout.Label(selectionSize.ToString());
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
        }

        GUILayout.EndArea();
    }
}
