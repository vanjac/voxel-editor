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

        panelRect = new Rect(190, 10, scaledScreenWidth - 190, 20);

        if (voxelArray.selectMode != VoxelArray.SelectMode.NONE)
        {
            if (GUI.Button(new Rect(panelRect.xMin, panelRect.yMin, 120, 20), "Apply Selection"))
            {
                voxelArray.StoreSelection();
            }
        }
        else if (voxelArray.SomethingIsSelected())
        {
            if (GUI.Button(new Rect(panelRect.xMin, panelRect.yMin, 120, 20), "Clear Selection"))
            {
                voxelArray.ClearStoredSelection();
                voxelArray.ClearSelection();
            }
        }

        if (GUI.Button(new Rect(panelRect.xMin + 130, panelRect.yMin, 80, 20), "Play"))
        {

            editorFile.Play();
        }

        if (GUI.Button(new Rect(panelRect.xMin + 220, panelRect.yMin, 80, 20), "Close"))
        {
            editorFile.Close();
        }

        Vector3 selectionSize = voxelArray.selectionBounds.size;
        if (selectionSize != Vector3.zero)
        {
            GUI.skin.label.alignment = TextAnchor.LowerRight;
            GUI.Label(new Rect(panelRect.xMin, targetHeight - 24, panelRect.width - 10, 24), selectionSize.ToString());
            GUI.skin.label.alignment = TextAnchor.UpperRight;
        }
    }
}
