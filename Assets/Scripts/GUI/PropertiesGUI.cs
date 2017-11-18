using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropertiesGUI : GUIPanel {

    public VoxelArray voxelArray;

    MaterialSelectorGUI materialSelector;

    public override void OnGUI()
    {
        base.OnGUI();

        panelRect = new Rect(0, 0, 180, targetHeight);

        GUI.Box(panelRect, "Properties");

        Rect scrollBox = new Rect(panelRect.xMin, panelRect.yMin + 25, panelRect.width, panelRect.height - 25);
        float scrollAreaWidth = panelRect.width - 1;
        float scrollAreaHeight = scrollBox.height - 1; // TODO
        Rect scrollArea = new Rect(0, 0, scrollAreaWidth, scrollAreaHeight);
        scroll = GUI.BeginScrollView(scrollBox, scroll, scrollArea);

        if (GUI.Button(new Rect(scrollArea.xMin + 10, scrollArea.yMin, scrollArea.width - 20, 20), "Set Material"))
        {
            if (materialSelector == null)
            {
                materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
                materialSelector.voxelArray = voxelArray;
            }
        }

        GUI.EndScrollView();
    }

}
