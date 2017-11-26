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

        Rect paddedScrollArea = new Rect(scrollArea.xMin + 10, scrollArea.yMin, scrollArea.width - 20, scrollArea.height);
        GUILayout.BeginArea(paddedScrollArea);

        GUILayout.Label("FACES:");

        if (GUILayout.Button("Set Material"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.voxelArray = voxelArray;
            materialSelector.handler = voxelArray.AssignMaterial;
        }

        if (GUILayout.Button("Set Overlay"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.voxelArray = voxelArray;
            materialSelector.materialDirectory = "GameAssets/Overlays";
            materialSelector.allowNullMaterial = true;
            materialSelector.handler = voxelArray.AssignOverlay;
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Left"))
        {
            voxelArray.OrientFaces(3);
        }

        if (GUILayout.Button("Right"))
        {
            voxelArray.OrientFaces(1);
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Flip H"))
        {
            voxelArray.OrientFaces(7);
        }

        if (GUILayout.Button("Flip V"))
        {
            voxelArray.OrientFaces(5);
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("MAP:");

        if (GUILayout.Button("Set Sky"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.voxelArray = voxelArray;
            materialSelector.materialDirectory = "GameAssets/Skies";
            materialSelector.handler = SetSkybox;
        }

        GUILayout.Label("Ambient light intensity:");

        float oldIntensity = RenderSettings.ambientIntensity;
        float newIntensity = GUILayout.HorizontalSlider(oldIntensity, 0, 3);
        if (newIntensity != oldIntensity)
            RenderSettings.ambientIntensity = newIntensity;

        GUILayout.Label("Sun intensity:");

        oldIntensity = RenderSettings.sun.intensity;
        newIntensity = GUILayout.HorizontalSlider(oldIntensity, 0, 3);
        if (newIntensity != oldIntensity)
            RenderSettings.sun.intensity = newIntensity;

        if (GUILayout.Button("Sun Color"))
        {
            ColorPickerGUI colorPicker = gameObject.AddComponent<ColorPickerGUI>();
            colorPicker.color = RenderSettings.sun.color;
            colorPicker.handler = SetSunColor;
        }

        GUILayout.EndArea();
        GUI.EndScrollView();
    }

    private void SetSkybox(Material sky)
    {
        RenderSettings.skybox = sky;
    }

    private void SetSunColor(Color color)
    {
        RenderSettings.sun.color = color;
    }

}
