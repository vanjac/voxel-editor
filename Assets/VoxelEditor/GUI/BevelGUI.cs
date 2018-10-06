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
            new string[] { "None", "Flat", "Curve", "Stair 1/2", "Stair 1/4", "Stair 1/8" },
            6, GUI.skin.GetStyle("button_tab"));
        var newBevelSize = (VoxelEdge.BevelSize)GUILayout.SelectionGrid((int)voxelEdge.bevelSize,
            new string[] { "Full", "Half", "Quarter" },
            3, GUI.skin.GetStyle("button_tab"));
        if (newBevelType != voxelEdge.bevelType || newBevelSize != voxelEdge.bevelSize)
        {
            voxelEdge.bevelType = newBevelType;
            voxelEdge.bevelSize = newBevelSize;
            voxelArray.TEMP_SetEdges(voxelEdge, edgeNum);
        }
    }
}
