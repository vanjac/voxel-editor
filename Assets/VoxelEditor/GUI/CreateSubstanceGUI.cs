using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSubstanceGUI : ActionBarGUI
{
    public override void OnEnable()
    {
        base.OnEnable();
        stealFocus = true;
        ActionBarGUI actionBar = GetComponent<ActionBarGUI>();
        if (actionBar != null)
        {
            actionBar.enabled = false;
            closeIcon = actionBar.closeIcon;
            applySelectionIcon = actionBar.applySelectionIcon;
            clearSelectionIcon = actionBar.clearSelectionIcon;
        }
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

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();

        if (ActionBarButton(closeIcon))
        {
            voxelArray.substanceToCreate = null;
            Destroy(this);
        }
        else if (voxelArray.substanceToCreate == null)
        {
            Destroy(this);
        }

        SelectionGUI();
        GUILayout.FlexibleSpace();
        ActionBarLabel("Push or pull to create a substance");

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
    }
}
