using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .25f, height * .25f, width * .5f, height * .5f);
    }

    public override void WindowGUI()
    {
        if (GUILayout.Button("Set Material"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.allowNullMaterial = true; // TODO: disable if no substances selected
            materialSelector.handler = voxelArray.AssignMaterial;
        }

        if (GUILayout.Button("Set Overlay"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
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
            voxelArray.OrientFaces(5);
        }

        if (GUILayout.Button("Flip V"))
        {
            voxelArray.OrientFaces(7);
        }

        GUILayout.EndHorizontal();
    }
}