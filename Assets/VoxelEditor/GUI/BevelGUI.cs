using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BevelActionBarGUI : ActionBarGUI
{
    private BevelGUI bevelGUI;

    public override void OnEnable()
    {
        base.OnEnable();
        stealFocus = true;
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void Start()
    {
        base.Start();
        bevelGUI = gameObject.AddComponent<BevelGUI>();
        bevelGUI.voxelArray = voxelArray;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(bevelGUI);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        ActionBarLabel("Bevels");
        GUILayout.FlexibleSpace();
        if (HighlightedActionBarButton(GUIIconSet.instance.done))
            Destroy(this);
        GUILayout.EndHorizontal();
    }
}

public class BevelGUI : LeftPanelGUI
{
    public VoxelArrayEditor voxelArray;

    private int edgeNum = 0;
    private VoxelEdge voxelEdge;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(0, 0, height * .6f, height);
    }

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    public override void Start()
    {
        base.Start();
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

        GUILayout.Label("Shape:");
        var newBevelType = (VoxelEdge.BevelType)GUILayout.SelectionGrid((int)voxelEdge.bevelType,
            new Texture[] {
                GUIIconSet.instance.x,
                GUIIconSet.instance.bevelIcons.square,
                GUIIconSet.instance.bevelIcons.flat,
                GUIIconSet.instance.bevelIcons.curve,
                GUIIconSet.instance.bevelIcons.stair2,
                GUIIconSet.instance.bevelIcons.stair4 },
            3, GUI.skin.GetStyle("button_tab"));
        GUILayout.Label("Size:");
        var newBevelSize = (VoxelEdge.BevelSize)GUILayout.SelectionGrid((int)voxelEdge.bevelSize,
            new Texture[] {
                GUIIconSet.instance.bevelIcons.quarter,
                GUIIconSet.instance.bevelIcons.half,
                GUIIconSet.instance.bevelIcons.full },
            3, GUI.skin.GetStyle("button_tab"));
        GUILayout.BeginHorizontal();
        var newAddSelected = GUILayout.Toggle(voxelEdge.addSelected, "+Select", GUI.skin.button);
        var newStoredSelected = GUILayout.Toggle(voxelEdge.storedSelected, ".Select", GUI.skin.button);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
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
