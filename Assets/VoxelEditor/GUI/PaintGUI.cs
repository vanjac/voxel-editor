using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintGUI : GUIPanel
{
    private const int PREVIEW_SIZE = 250;

    public delegate void PaintHandler(VoxelFace paint);

    public PaintHandler handler;
    public VoxelFace paint;

    private Rect windowRect;
    private int selectedLayer = 0;
    private MaterialSelectorGUI materialSelector;

    private GUIStyle condensedButtonStyle = null;

    public override Rect GetRect(float width, float height)
    {
        windowRect = new Rect(width * .15f, height * .05f, width * .7f, height * .9f);
        return windowRect;
    }

    void Start()
    {
        UpdateMaterialSelector();
    }

    public override void WindowGUI()
    {
        if (condensedButtonStyle == null)
        {
            condensedButtonStyle = new GUIStyle(GUI.skin.button);
            condensedButtonStyle.padding.left = 16;
            condensedButtonStyle.padding.right = 16;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(PREVIEW_SIZE), GUILayout.Height(PREVIEW_SIZE));
        DrawPaint(paint, GUILayoutUtility.GetLastRect());
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(GUIIconSet.instance.rotateLeft, condensedButtonStyle, GUILayout.ExpandWidth(false)))
            Orient(3);
        if (GUILayout.Button(GUIIconSet.instance.rotateRight, condensedButtonStyle, GUILayout.ExpandWidth(false)))
            Orient(1);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Done"))
            Destroy(this);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(GUIIconSet.instance.flipHorizontal, condensedButtonStyle, GUILayout.ExpandWidth(false)))
            Orient(5);
        if (GUILayout.Button(GUIIconSet.instance.flipVertical, condensedButtonStyle, GUILayout.ExpandWidth(false)))
            Orient(7);
        int oldSelectedLayer = selectedLayer;
        selectedLayer = GUILayout.SelectionGrid(
            selectedLayer, new string[] { "Material", "Overlay" }, 2, condensedButtonStyle);
        if (oldSelectedLayer != selectedLayer)
            UpdateMaterialSelector();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if (materialSelector != null)
        {
            materialSelector.scroll = scroll;
            materialSelector.WindowGUI();
            scroll = materialSelector.scroll;
        }
    }

    private void UpdateMaterialSelector()
    {
        if (materialSelector != null)
            Destroy(materialSelector);
        materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
        materialSelector.enabled = false;
        materialSelector.allowNullMaterial = true; // TODO: disable if no substances selected
        materialSelector.closeOnSelect = false;
        if (selectedLayer == 0)
        {
            materialSelector.rootDirectory = "GameAssets/Materials";
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
            materialSelector.rootDirectory = "GameAssets/Overlays";
            materialSelector.allowAlpha = true;
            materialSelector.handler = (Material mat) =>
            {
                if (mat != null || paint.material != null)
                    paint.overlay = mat;
                handler(paint);
            };
            materialSelector.highlightMaterial = paint.overlay;
        }
        materialSelector.Start(); // not enabled so wouldn't be called normally
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
        Matrix4x4 baseMatrix = GUI.matrix;
        Vector2 translation = rect.center + windowRect.min;
        GUI.matrix *= Matrix4x4.Translate(translation);
        float rotation = VoxelFace.GetOrientationRotation(paint.orientation) * 90;
        if (VoxelFace.GetOrientationMirror(paint.orientation))
        {
            GUI.matrix *= Matrix4x4.Scale(new Vector3(-1, 1, 1));
            rotation += 90;
        }
        GUI.matrix *= Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, 0, rotation)));
        GUI.matrix *= Matrix4x4.Translate(-translation);
        MaterialSelectorGUI.DrawMaterialTexture(paint.material, rect, false);
        MaterialSelectorGUI.DrawMaterialTexture(paint.overlay, rect, true);
        GUI.matrix = baseMatrix;
    }
}
