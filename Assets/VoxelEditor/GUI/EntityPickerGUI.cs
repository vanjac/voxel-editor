using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPickerGUI : ActionBarGUI
{
    public delegate void EntityPickerHanlder(ICollection<Entity> entities);
    public EntityPickerHanlder handler;
    public bool allowNone = true, allowMultiple = true;

    private VoxelArrayEditor.SelectionState selectionState;

    public override void OnEnable()
    {
        base.OnEnable();
        stealFocus = true;
        ActionBarGUI actionBar = GetComponent<ActionBarGUI>();
        if (actionBar != null)
            actionBar.enabled = false;
        propertiesGUI.normallyOpen = false; // hide properties panel
    }

    public override void OnDisable()
    {
        base.OnDisable();
        ActionBarGUI actionBar = GetComponent<ActionBarGUI>();
        if (actionBar != null)
            actionBar.enabled = true;
        propertiesGUI.normallyOpen = true; // show properties panel
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

        if (ActionBarButton(GUIIconSet.instance.close))
            Destroy(this);

        if (allowMultiple)
            SelectionGUI();
        GUILayout.FlexibleSpace();

        // TODO: not efficient to keep generating a list of selected entities
        int numSelectedEntities = voxelArray.GetSelectedEntities().Count;
        if (numSelectedEntities == 0)
            ActionBarLabel("Pick an object...");
        else
            ActionBarLabel(numSelectedEntities + " objects selected");

        GUILayout.FlexibleSpace();

        bool ready = true;
        if (!allowNone && numSelectedEntities == 0)
            ready = false;
        if (!allowMultiple && numSelectedEntities > 1)
            ready = false;
        if (ready && HighlightedActionBarButton(GUIIconSet.instance.done))
        {
            handler(voxelArray.GetSelectedEntities());
            Destroy(this);
        }

        GUILayout.EndHorizontal();
    }
}
