using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BevelGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;

    private int edgeNum = 0;
    private VoxelEdge voxelEdge;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .2f, height * .1f, width * .6f, 0);
    }

    public void Start()
    {
        voxelEdge = voxelArray.TEMP_GetSelectedEdge(edgeNum);
    }

    public override void WindowGUI()
    {
        int newEdgeNum = GUILayout.SelectionGrid(edgeNum,
            new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" },
            4, GUI.skin.GetStyle("button_tab"));
        if (newEdgeNum != edgeNum)
        {
            edgeNum = newEdgeNum;
            voxelEdge = voxelArray.TEMP_GetSelectedEdge(edgeNum);
        }

        var newBevelType = (VoxelEdge.BevelType)GUILayout.SelectionGrid((int)voxelEdge.bevelType,
            new string[] { "None", "Square", "Flat", "Curve", "2 Stair", "4 Stair" },
            6, GUI.skin.GetStyle("button_tab"));
        var newBevelSize = (VoxelEdge.BevelSize)GUILayout.SelectionGrid((int)voxelEdge.bevelSize,
            new string[] { "Quarter", "Half", "Full" },
            3, GUI.skin.GetStyle("button_tab"));
        GUILayout.BeginHorizontal();
        var newAddSelected = GUILayout.Toggle(voxelEdge.addSelected, "+Select", GUI.skin.button);
        var newStoredSelected = GUILayout.Toggle(voxelEdge.storedSelected, ".Select", GUI.skin.button);
        var newCapMin = GUILayout.Toggle(voxelEdge.capMin, "Cap Min", GUI.skin.button);
        var newCapMax = GUILayout.Toggle(voxelEdge.capMax, "Cap Max", GUI.skin.button);
        GUILayout.EndHorizontal();
        if (newBevelType != voxelEdge.bevelType || newBevelSize != voxelEdge.bevelSize
            || newAddSelected != voxelEdge.addSelected || newStoredSelected != voxelEdge.storedSelected
            || newCapMin != voxelEdge.capMin || newCapMax != voxelEdge.capMax)
        {
            voxelEdge.bevelType = newBevelType;
            voxelEdge.bevelSize = newBevelSize;
            voxelEdge.addSelected = newAddSelected;
            voxelEdge.storedSelected = newStoredSelected;
            voxelEdge.capMin = newCapMin;
            voxelEdge.capMax = newCapMax;
            voxelArray.TEMP_SetEdges(voxelEdge, edgeNum);
        }
    }
}
