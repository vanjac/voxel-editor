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
    private Texture2D whiteTexture;

    public override Rect GetRect(float width, float height)
    {
        windowRect = new Rect(width * .25f, height * .1f, width * .5f, height * .8f);
        return windowRect;
    }

    void Start()
    {
        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(PREVIEW_SIZE), GUILayout.Height(PREVIEW_SIZE));
        DrawPaint(paint, GUILayoutUtility.GetLastRect());
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Left"))
            Orient(3);
        if (GUILayout.Button("Right"))
            Orient(1);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Flip H"))
            Orient(5);
        if (GUILayout.Button("Flip V"))
            Orient(7);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Change Material"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.title = "Change Material";
            materialSelector.allowNullMaterial = true; // TODO: disable if no substances selected
            materialSelector.handler = (Material mat) =>
            {
                if (mat != null || paint.overlay != null)
                    paint.material = mat;
                handler(paint);
            };
        }

        if (GUILayout.Button("Change Overlay"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.title = "Change Overlay";
            materialSelector.materialDirectory = "GameAssets/Overlays";
            materialSelector.allowNullMaterial = true;
            materialSelector.handler = (Material mat) =>
            {
                if (mat != null || paint.material != null)
                    paint.overlay = mat;
                handler(paint);
            };
        }
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
        DrawMaterialTexture(paint.material, rect, false);
        DrawMaterialTexture(paint.overlay, rect, true);
        GUI.matrix = baseMatrix;
    }

    private void DrawMaterialTexture(Material mat, Rect rect, bool alpha)
    {
        if (mat == null)
            return;
        Rect texCoords = new Rect(Vector2.zero, Vector2.one);
        Texture texture = whiteTexture;
        if (mat.mainTexture != null)
        {
            texture = mat.mainTexture;
            texCoords = new Rect(Vector2.zero, mat.mainTextureScale);
        }
        else if (mat.HasProperty("_ColorControl"))
            // water shader
            texture = mat.GetTexture("_ColorControl");
        else if (mat.HasProperty("_FrontTex"))
            // skybox
            texture = mat.GetTexture("_FrontTex");

        Color baseColor = GUI.color;
        if (mat.HasProperty("_Color"))
            GUI.color *= mat.color;
        GUI.DrawTextureWithTexCoords(rect, texture, texCoords, alpha);
        GUI.color = baseColor;
    }
}