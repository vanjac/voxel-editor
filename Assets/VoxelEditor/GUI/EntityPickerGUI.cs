using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPickerGUI : GUIPanel
{
    public delegate void EntityPickerHanlder(ICollection<Entity> entities);

    public VoxelArrayEditor voxelArray;
    public EntityPickerHanlder handler;

    private VoxelArrayEditor.SelectionState selectionState;

    public override void OnEnable()
    {
        holdOpen = true;
        ActionBarGUI actionBar = GetComponent<ActionBarGUI>();
        if (actionBar != null)
            actionBar.enabled = false;
        base.OnEnable();
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

    public override Rect GetRect(float width, float height)
    {
        return new Rect(height * .5f, 0, width - height * .5f, 0);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        // TODO: not efficient to keep generating a list of selected entities
        int numSelectedEntities = voxelArray.GetSelectedEntities().Count;
        if (numSelectedEntities == 0)
            GUILayout.Label("Pick an object... ", GUILayout.ExpandWidth(false));
        else
            GUILayout.Label(numSelectedEntities + " objects selected ", GUILayout.ExpandWidth(false));

        // from ActionBarGUI...

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

        // Toggle that looks like a button, but with On style
        if (!GUILayout.Toggle(true, "Done", GUI.skin.button, GUILayout.ExpandWidth(false)))
        {
            handler(voxelArray.GetSelectedEntities());
            Destroy(this);
        }

        GUILayout.EndHorizontal();
    }
}
