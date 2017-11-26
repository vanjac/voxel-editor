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
        float scrollAreaHeight = scrollBox.height * 1.5f; // TODO
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

        float oldValue = RenderSettings.ambientIntensity;
        float newValue = GUILayout.HorizontalSlider(oldValue, 0, 3);
        if (newValue != oldValue)
        {
            RenderSettings.ambientIntensity = newValue;
            voxelArray.unsavedChanges = true;
        }

        GUILayout.Label("Sun intensity:");

        oldValue = RenderSettings.sun.intensity;
        newValue = GUILayout.HorizontalSlider(oldValue, 0, 3);
        if (newValue != oldValue)
        {
            RenderSettings.sun.intensity = newValue;
            voxelArray.unsavedChanges = true;
        }

        if (GUILayout.Button("Sun Color"))
        {
            ColorPickerGUI colorPicker = gameObject.AddComponent<ColorPickerGUI>();
            colorPicker.color = RenderSettings.sun.color;
            colorPicker.handler = SetSunColor;
        }

        GUILayout.Label("Sun Pitch:");

        oldValue = RenderSettings.sun.transform.rotation.eulerAngles.x;
        if (oldValue > 270)
            oldValue -= 360;
        newValue = GUILayout.HorizontalSlider(oldValue, -90, 90);
        if (newValue != oldValue)
        {
            Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
            eulerAngles.x = newValue;
            RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);
            voxelArray.unsavedChanges = true;
        }

        GUILayout.Label("Sun Yaw:");

        oldValue = RenderSettings.sun.transform.rotation.eulerAngles.y;
        newValue = GUILayout.HorizontalSlider(oldValue, 0, 360);
        if (newValue != oldValue)
        {
            Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
            eulerAngles.y = newValue;
            RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);
            voxelArray.unsavedChanges = true;
        }

        GUILayout.EndArea();
        GUI.EndScrollView();
    }

    private void SetSkybox(Material sky)
    {
        RenderSettings.skybox = sky;
        voxelArray.unsavedChanges = true;
    }

    private void SetSunColor(Color color)
    {
        RenderSettings.sun.color = color;
        voxelArray.unsavedChanges = true;
    }

}
