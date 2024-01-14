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
        bevelGUI.touchListener = touchListener;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(bevelGUI);
    }

    public override void WindowGUI()
    {
        // clear substance highlight while properties panel is disabled
        EntityReferencePropertyManager.Reset(null);

        GUILayout.BeginHorizontal();
        SelectionGUI();
        GUILayout.FlexibleSpace();

        Vector3 selectionSize = voxelArray.selectionBounds.size;
        if (selectionSize == Vector3.zero)
            ActionBarLabel(StringSet.BevelSelectEdgesInstruction);
        else
            ActionBarLabel(SelectionString(selectionSize));

        GUILayout.FlexibleSpace();
        TutorialGUI.TutorialHighlight("bevel done");
        if (HighlightedActionBarButton(IconSet.done))
            Destroy(this);
        TutorialGUI.ClearHighlight();
        GUILayout.EndHorizontal();
    }
}


public class BevelGUI : LeftPanelGUI
{
    public VoxelArrayEditor voxelArray;
    public TouchListener touchListener;

    private VoxelEdge voxelEdge;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        new Rect(safeRect.xMin, safeRect.yMin, 540, 0);

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
        EnableEdgeMode();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        touchListener.selectType = VoxelElement.FACES;
        voxelArray.ClearStoredSelection();
        voxelArray.ClearSelection();
    }

    public override void Start()
    {
        base.Start();
        EnableEdgeMode();
    }

    private void EnableEdgeMode()
    {
        if (touchListener != null)
            touchListener.selectType = VoxelElement.EDGES;
        if (voxelArray != null)
        {
            voxelArray.ClearStoredSelection();
            voxelArray.ClearSelection();
        }
    }

    public override void WindowGUI()
    {
        if (voxelArray.selectionChanged)
        {
            voxelArray.selectionChanged = false;
            voxelEdge = voxelArray.GetSelectedBevel();
        }

        GUILayout.Label(StringSet.BevelHeader, StyleSet.labelTitle);

        if (!voxelArray.SomethingIsSelected())
        {
            GUILayout.Label(StringSet.BevelNoSelection);
            return;
        }

        TutorialGUI.TutorialHighlight("bevel shape");
        GUILayout.Label(StringSet.BevelShapeHeader);
        var newBevelType = (VoxelEdge.BevelType)GUILayout.SelectionGrid((int)voxelEdge.bevelType,
            new Texture[] {
                IconSet.no,
                IconSet.bevelIcons.flat,
                IconSet.bevelIcons.curve,
                IconSet.bevelIcons.square,
                IconSet.bevelIcons.stair2,
                IconSet.bevelIcons.stair4 },
            3, StyleSet.buttonTab);
        TutorialGUI.ClearHighlight();

        TutorialGUI.TutorialHighlight("bevel size");
        GUILayout.Label(StringSet.BevelSizeHeader);
        var newBevelSize = (VoxelEdge.BevelSize)GUILayout.SelectionGrid((int)voxelEdge.bevelSize,
            new Texture[] {
                IconSet.bevelIcons.quarter,
                IconSet.bevelIcons.half,
                IconSet.bevelIcons.full },
            3, StyleSet.buttonTab);
        TutorialGUI.ClearHighlight();

        if (newBevelType != voxelEdge.bevelType || newBevelSize != voxelEdge.bevelSize)
        {
            voxelEdge.bevelType = newBevelType;
            voxelEdge.bevelSize = newBevelSize;
            voxelArray.BevelSelectedEdges(voxelEdge);
        }
    }
}
