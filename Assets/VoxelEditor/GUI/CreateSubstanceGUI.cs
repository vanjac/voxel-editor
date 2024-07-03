using UnityEngine;

public class CreateSubstanceGUI : ActionBarGUI {
    public override void OnEnable() {
        base.OnEnable();
        stealFocus = true;
        GetComponent<PropertiesGUI>().normallyOpen = false; // hide properties panel
    }

    public override void OnDisable() {
        base.OnDisable();
        GetComponent<PropertiesGUI>().normallyOpen = true; // show properties panel
    }

    public override void WindowGUI() {
        GUILayout.BeginHorizontal();

        if (ActionBarButton(IconSet.close)) {
            voxelArray.substanceToCreate = null;
            Destroy(this);
        } else if (voxelArray.substanceToCreate == null) {
            Destroy(this);
        }

        SelectionGUI();
        GUILayout.FlexibleSpace();
        ActionBarLabel(StringSet.CreateSubstanceInstruction);

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
    }
}
