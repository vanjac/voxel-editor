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
        base.OnEnable();
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
        return new Rect(height * .55f, height * .9f, height * .65f, height * .1f);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Pick an object...");
        if (GUILayout.Button("Done"))
        {
            handler(voxelArray.GetSelectedEntities());
            Destroy(this);
        }
        GUILayout.EndHorizontal();
    }
}
