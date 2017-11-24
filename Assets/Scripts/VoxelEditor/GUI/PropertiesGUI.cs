using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropertiesGUI : GUIPanel {

    public VoxelArray voxelArray;

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
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.voxelArray = voxelArray;
            materialSelector.handler = voxelArray.AssignMaterial;
        }

        if (GUI.Button(new Rect(scrollArea.xMin + 10, scrollArea.yMin + 25, scrollArea.width - 20, 20), "Set Overlay"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.voxelArray = voxelArray;
            materialSelector.materialDirectory = "GameAssets/Overlays";
            materialSelector.allowNullMaterial = true;
            materialSelector.handler = voxelArray.AssignOverlay;
        }

        if (GUI.Button(new Rect(scrollArea.xMin + 10, scrollArea.yMin + 50, (scrollArea.width - 20) / 2, 20), "Left"))
        {
            voxelArray.OrientFaces(3);
        }

        if (GUI.Button(new Rect(scrollArea.xMin + 10 + (scrollArea.width - 20) / 2, scrollArea.yMin + 50, (scrollArea.width - 20) / 2, 20), "Right"))
        {
            voxelArray.OrientFaces(1);
        }

        if (GUI.Button(new Rect(scrollArea.xMin + 10, scrollArea.yMin + 75, (scrollArea.width - 20) / 2, 20), "Flip H"))
        {
            voxelArray.OrientFaces(7);
        }

        if (GUI.Button(new Rect(scrollArea.xMin + 10 + (scrollArea.width - 20) / 2, scrollArea.yMin + 75, (scrollArea.width - 20) / 2, 20), "Flip V"))
        {
            voxelArray.OrientFaces(5);
        }

        if (GUI.Button(new Rect(scrollArea.xMin + 10, scrollArea.yMin + 100, scrollArea.width - 20, 20), "Set Sky"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.voxelArray = voxelArray;
            materialSelector.materialDirectory = "GameAssets/Skies";
            materialSelector.handler = SetSkybox;
        }

        GUI.EndScrollView();
    }

    private void SetSkybox(Material sky)
    {
        RenderSettings.skybox = sky;
    }

}
