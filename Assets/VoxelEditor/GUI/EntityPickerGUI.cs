using System.Collections.Generic;
using UnityEngine;

public class EntityPickerGUI : ActionBarGUI {
    public System.Action<ICollection<Entity>> handler;
    public bool allowNone = true, allowMultiple = true, allowNull = false;
    public string nullName = StringSet.EntityPickNone;

    private VoxelArrayEditor.SelectionState selectionState;

    public override void OnEnable() {
        // copied from CreateSubstanceGUI
        base.OnEnable();
        stealFocus = true;
        GetComponent<PropertiesGUI>().normallyOpen = false; // hide properties panel
    }

    public override void OnDisable() {
        // copied from CreateSubstanceGUI
        base.OnDisable();
        GetComponent<PropertiesGUI>().normallyOpen = true; // show properties panel
    }

    public override void Start() {
        base.Start();
        GetComponent<PropertiesGUI>().freezeUpdates = true; // prevent panel resetting scroll
        selectionState = voxelArray.GetSelectionState();
        voxelArray.ClearSelection();
        voxelArray.ClearStoredSelection();
    }

    public override void OnDestroy() {
        base.OnDestroy();
        voxelArray.RecallSelectionState(selectionState);
        voxelArray.selectionChanged = false; // prevent panel resetting scroll
        GetComponent<PropertiesGUI>().freezeUpdates = false;
    }

    public override void WindowGUI() {
        GUILayout.BeginHorizontal();

        if (ActionBarButton(IconSet.close)) {
            Destroy(this);
        }

        if (allowMultiple) {
            SelectionGUI();
        }

        if (allowNull) {
            if (ActionBarButton(nullName)) {
                handler(new Entity[] { null });
                Destroy(this);
            }
        }

        GUILayout.FlexibleSpace();

        // TODO: not efficient to keep generating a list of selected entities
        int numSelectedEntities = voxelArray.GetSelectedEntities().Count;
        if (numSelectedEntities == 0) {
            ActionBarLabel(StringSet.PickObjectInstruction);
        } else {
            ActionBarLabel(StringSet.PickObjectCount(numSelectedEntities));
        }

        GUILayout.FlexibleSpace();

        bool ready = true;
        if (!allowNone && numSelectedEntities == 0) {
            ready = false;
        }
        if (!allowMultiple && numSelectedEntities > 1) {
            ready = false;
        }
        if (ready && HighlightedActionBarButton(IconSet.done)) {
            handler(voxelArray.GetSelectedEntities());
            Destroy(this);
        }

        GUILayout.EndHorizontal();
    }
}
