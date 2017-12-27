using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPickerGUI : ActionBarGUI
{
    public delegate void EntityPickerHanlder(ICollection<Entity> entities);
    public EntityPickerHanlder handler;

    private VoxelArrayEditor.SelectionState selectionState;

    public override void OnEnable()
    {
        base.OnEnable();
        stealFocus = true;
        ActionBarGUI actionBar = GetComponent<ActionBarGUI>();
        if (actionBar != null) {
            actionBar.enabled = false;
            closeIcon = actionBar.closeIcon;
            applySelectionIcon = actionBar.applySelectionIcon;
            clearSelectionIcon = actionBar.clearSelectionIcon;
            doneIcon = actionBar.doneIcon;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        ActionBarGUI actionBar = GetComponent<ActionBarGUI>();
        if (actionBar != null)
            actionBar.enabled = true;
    }

    void Start()
    {
        selectionState = voxelArray.GetSelectionState();
        voxelArray.ClearSelection();
        voxelArray.ClearStoredSelection();
    }

    void OnDestroy()
    {
        voxelArray.RecallSelectionState(selectionState);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();

        if (ActionBarButton(closeIcon))
            Destroy(this);

        // TODO: not efficient to keep generating a list of selected entities
        string labelText;
        int numSelectedEntities = voxelArray.GetSelectedEntities().Count;
        if (numSelectedEntities == 0)
            labelText = "Pick an object...";
        else
            labelText = numSelectedEntities + " objects selected";
        GUILayout.Label(labelText, labelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));

        SelectionGUI();

        if (HighlightedActionBarButton(doneIcon))
        {
            handler(voxelArray.GetSelectedEntities());
            Destroy(this);
        }

        GUILayout.EndHorizontal();
    }
}
