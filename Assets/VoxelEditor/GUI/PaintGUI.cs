﻿using System.Collections;
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

    public delegate void PaintHandler(VoxelFace paint);

    public PaintHandler handler;
    public VoxelFace paint;
    public VoxelArrayEditor voxelArray;

    private PaintLayer selectedLayer = PaintLayer.MATERIAL;
    private MaterialSelectorGUI materialSelector;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            safeRect.width * .7f, safeRect.height * .9f, maxWidth: 1360);
    }

    public override void OnEnable()
    {
        showCloseButton = true;
        base.OnEnable();
    }

    void Start()
    {
        if (paint.overlay != null)
            selectedLayer = PaintLayer.OVERLAY;
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
        if (GUILayout.Button(GUIIconSet.instance.rotateLeft, GUIStyleSet.instance.buttonSmall, GUILayout.ExpandWidth(false)))
            Orient(3);
        // BeginHorizontalClipped prevents recent paints from expanding window
        // it's important that one button is outside the view to set the correct height
        // and that one button is always inside the view or weird buggy behavior happens
        GUIUtils.BeginHorizontalClipped(GUILayout.ExpandHeight(false));
        if (GUILayout.Button(GUIIconSet.instance.rotateRight, GUIStyleSet.instance.buttonSmall, GUILayout.ExpandWidth(false)))
            Orient(1);
        TutorialGUI.ClearHighlight();

        GUILayout.FlexibleSpace();
        foreach (VoxelFace recentPaint in recentPaints)
        {
            if (GUILayout.Button(" ", GUIStyleSet.instance.buttonSmall,
                GUILayout.Width(RECENT_PREVIEW_SIZE), GUILayout.Height(RECENT_PREVIEW_SIZE)))
            {
                paint = recentPaint;
                handler(paint);
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
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        TutorialGUI.TutorialHighlight("paint transform");
        if (GUILayout.Button(GUIIconSet.instance.flipHorizontal, GUIStyleSet.instance.buttonSmall, GUILayout.ExpandWidth(false)))
            Orient(5);
        if (GUILayout.Button(GUIIconSet.instance.flipVertical, GUIStyleSet.instance.buttonSmall, GUILayout.ExpandWidth(false)))
            Orient(7);
        TutorialGUI.ClearHighlight();
        PaintLayer oldSelectedLayer = selectedLayer;
        TutorialGUI.TutorialHighlight("paint layer");
        selectedLayer = (PaintLayer)GUILayout.SelectionGrid(
            (int)selectedLayer, new string[] { "Material", "Overlay" }, 2,
            GUIStyleSet.instance.buttonSmall);
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

    private void UpdateMaterialSelector()
    {
        if (materialSelector != null)
            Destroy(materialSelector);
        materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
        materialSelector.enabled = false;
        materialSelector.voxelArray = voxelArray;
        materialSelector.allowNullMaterial = true; // TODO: disable if no substances selected
        materialSelector.layer = selectedLayer;
        if (selectedLayer == PaintLayer.MATERIAL)
        {
            materialSelector.handler = (Material mat) =>
            {
                if (mat != null || paint.overlay != null)
                    paint.material = mat;
                handler(paint);
            };
            materialSelector.highlightMaterial = paint.material;
        }
        else
        {
            materialSelector.handler = (Material mat) =>
            {
                if (mat != null || paint.material != null)
                    paint.overlay = mat;
                handler(paint);
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
        handler(paint);
    }

    private void DrawPaint(VoxelFace paint, Rect rect)
    {
        float rotation = VoxelFace.GetOrientationRotation(paint.orientation) * 90;
        Vector2 scaleFactor = Vector2.one;
        if (VoxelFace.GetOrientationMirror(paint.orientation))
        {
            scaleFactor = new Vector2(-1, 1);
            rotation += 90;
        }
        Matrix4x4 baseMatrix = GUI.matrix;
        RotateAboutPoint(rect.center, rotation, scaleFactor);
        MaterialSelectorGUI.DrawMaterialTexture(paint.material, rect);
        MaterialSelectorGUI.DrawMaterialTexture(paint.overlay, rect);
        GUI.matrix = baseMatrix;
    }

    public void TutorialShowSky()
    {
        selectedLayer = PaintLayer.MATERIAL;
        paint.material = ResourcesDirectory.FindMaterial("Sky", true);
        handler(paint);
        UpdateMaterialSelector();
    }
}
