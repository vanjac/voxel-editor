using System.Collections.Generic;
using UnityEngine;

public class PaintGUI : GUIPanel
{
    private const int PREVIEW_SIZE = 224;
    private const int NUM_RECENT_PAINTS = 5;
    private const int RECENT_PREVIEW_SIZE = 96;
    private const int RECENT_MARGIN = 12;
    private static readonly int[] COARSE_SIN = { 0, 1, 0, -1 };
    private static readonly int[] COARSE_COS = { 1, 0, -1, 0 };

    private static List<VoxelFace> recentPaints = new List<VoxelFace>(); // most recent first

    public System.Action<VoxelFace> handler;
    public VoxelFace paint;
    public VoxelArrayEditor voxelArray;

    private int selectedLayer = 0;
    private MaterialSelectorGUI materialSelector;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            safeRect.width * .7f, safeRect.height * .9f, maxWidth: 1360);

    void Start()
    {
        if (paint.overlay != null)
            selectedLayer = 1;
        UpdateMaterialSelector();
    }

    void OnDestroy()
    {
        // add to recent materials list
        for (int i = recentPaints.Count - 1; i >= 0; i--)
            if (recentPaints[i].Equals(paint))
                recentPaints.RemoveAt(i);
        recentPaints.Insert(0, paint);
        while (recentPaints.Count > NUM_RECENT_PAINTS)
            recentPaints.RemoveAt(recentPaints.Count - 1);

        if (materialSelector != null)
            Destroy(materialSelector);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(PREVIEW_SIZE), GUILayout.Height(PREVIEW_SIZE));
        DrawPaint(paint, GUILayoutUtility.GetLastRect());
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        TutorialGUI.TutorialHighlight("paint transform");
        if (GUILayout.Button(IconSet.rotateLeft, StyleSet.buttonSmall, GUILayout.ExpandWidth(false)))
            Orient(3);
        // BeginHorizontalClipped prevents recent paints from expanding window
        // it's important that one button is outside the view to set the correct height
        // and that one button is always inside the view or weird buggy behavior happens
        GUIUtils.BeginHorizontalClipped(GUILayout.ExpandHeight(false));
        if (GUILayout.Button(IconSet.rotateRight, StyleSet.buttonSmall, GUILayout.ExpandWidth(false)))
            Orient(1);
        TutorialGUI.ClearHighlight();

        GUILayout.FlexibleSpace();
        foreach (VoxelFace recentPaint in recentPaints)
        {
            if (GUILayout.Button(" ", StyleSet.buttonSmall,
                GUILayout.Width(RECENT_PREVIEW_SIZE), GUILayout.Height(RECENT_PREVIEW_SIZE)))
            {
                paint = recentPaint;
                PaintChanged();
                UpdateMaterialSelector();
            }
            Rect buttonRect = GUILayoutUtility.GetLastRect();
            Rect paintRect = new Rect(
                buttonRect.xMin + RECENT_MARGIN, buttonRect.yMin + RECENT_MARGIN,
                buttonRect.width - RECENT_MARGIN * 2, buttonRect.height - RECENT_MARGIN * 2);
            DrawPaint(recentPaint, paintRect);
        }
        GUILayout.FlexibleSpace();

        GUIUtils.EndHorizontalClipped();
        if (GUILayout.Button("Done", GUILayout.ExpandWidth(false)))
            Destroy(this);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        TutorialGUI.TutorialHighlight("paint transform");
        if (GUILayout.Button(IconSet.flipHorizontal, StyleSet.buttonSmall, GUILayout.ExpandWidth(false)))
            Orient(5);
        if (GUILayout.Button(IconSet.flipVertical, StyleSet.buttonSmall, GUILayout.ExpandWidth(false)))
            Orient(7);
        TutorialGUI.ClearHighlight();
        int oldSelectedLayer = selectedLayer;
        TutorialGUI.TutorialHighlight("paint layer");
        selectedLayer = GUILayout.SelectionGrid(selectedLayer, new GUIContent[]
            {
                new GUIContent("  Material", IconSet.baseLayer),
                new GUIContent("  Overlay", IconSet.overlayLayer),
            }, 2, StyleSet.buttonSmall);
        TutorialGUI.ClearHighlight();
        if (oldSelectedLayer != selectedLayer)
            UpdateMaterialSelector();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if (materialSelector != null)
        {
            materialSelector.scroll = scroll;
            materialSelector.scrollVelocity = scrollVelocity;
            materialSelector.WindowGUI();
            scroll = materialSelector.scroll;
            scrollVelocity = materialSelector.scrollVelocity;
        }
        else
        {
            Destroy(this);
        }
    }

    private void PaintChanged()
    {
        if (paint.material != null || paint.overlay != null)
            handler(paint);
    }

    private void UpdateMaterialSelector()
    {
        if (materialSelector != null)
            Destroy(materialSelector);
        materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
        materialSelector.enabled = false;
        materialSelector.voxelArray = voxelArray;
        materialSelector.allowNullMaterial = true; // TODO: disable if no substances selected
        if (selectedLayer == 0)
        {
            materialSelector.rootDirectory = "Materials";
            materialSelector.handler = (Material mat) =>
            {
                paint.material = mat;
                PaintChanged();
            };
            materialSelector.highlightMaterial = paint.material;
        }
        else
        {
            materialSelector.rootDirectory = "Overlays";
            materialSelector.isOverlay = true;
            materialSelector.handler = (Material mat) =>
            {
                paint.overlay = mat;
                PaintChanged();
            };
            materialSelector.highlightMaterial = paint.overlay;
        }
        materialSelector.Start(); // not enabled so wouldn't be called normally
        scroll = Vector2.zero;
        scrollVelocity = Vector2.zero;
    }

    private void Orient(byte change)
    {
        int changeRotation = VoxelFace.GetOrientationRotation(change);
        bool changeFlip = VoxelFace.GetOrientationMirror(change);
        int paintRotation = VoxelFace.GetOrientationRotation(paint.orientation);
        bool paintFlip = VoxelFace.GetOrientationMirror(paint.orientation);
        if (paintFlip ^ changeFlip)
            paintRotation += 4 - changeRotation;
        else
            paintRotation += changeRotation;
        if (changeFlip)
            paintFlip = !paintFlip;
        paint.orientation = VoxelFace.Orientation(paintRotation, paintFlip);
        PaintChanged();
    }

    private void DrawPaint(VoxelFace paint, Rect rect)
    {
        int rotation = VoxelFace.GetOrientationRotation(paint.orientation);
        bool mirror = VoxelFace.GetOrientationMirror(paint.orientation);
        Vector2 u_vec = new Vector2(COARSE_COS[rotation], COARSE_SIN[rotation]);
        Vector2 v_vec = new Vector2(-COARSE_SIN[rotation], COARSE_COS[rotation]);
        if (VoxelFace.GetOrientationMirror(paint.orientation))
        {
            var tmp = u_vec;
            u_vec = v_vec * -1;
            v_vec = tmp * -1;
        }
        MaterialSelectorGUI.DrawMaterialTexture(paint.material, rect, false, u_vec, v_vec);
        MaterialSelectorGUI.DrawMaterialTexture(paint.overlay, rect, true, u_vec, v_vec);
    }

    public void TutorialShowSky()
    {
        selectedLayer = 0;
        paint.material = ResourcesDirectory.FindMaterial("Sky", true);
        PaintChanged();
        UpdateMaterialSelector();
    }
}
