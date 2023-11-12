using UnityEngine;

public class DrawSelectGUI : ActionBarGUI
{
    public override void Start()
    {
        base.Start();
        voxelArray.drawSelect = true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        voxelArray.drawSelect = false;
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        // done button will not be cut off if the buttons can't fit
        GUIUtils.BeginHorizontalClipped();
        // no stored selection
        if (voxelArray.SomethingIsSelected())
        {
            if (ActionBarButton(IconSet.clearSelection))
            {
                voxelArray.ClearStoredSelection();
                voxelArray.ClearSelection();
            }
        }
        EditGUI("Tap and drag to select");
        GUILayout.FlexibleSpace();
        GUIUtils.EndHorizontalClipped();
        if (HighlightedActionBarButton(IconSet.done))
            Destroy(this);
        GUILayout.EndHorizontal();
    }
}