using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintGUI : GUIPanel
{
    private const int PREVIEW_SIZE = 250;

    public VoxelFace paint;

    public VoxelArrayEditor voxelArray;

    private Rect windowRect;

    public override Rect GetRect(float width, float height)
    {
        windowRect = new Rect(width * .25f, height * .1f, width * .5f, height * .8f);
        return windowRect;
    }

    public override void WindowGUI()
    {
        GUILayout.BeginVertical(GUILayout.Height(PREVIEW_SIZE));
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(PREVIEW_SIZE), GUILayout.Height(PREVIEW_SIZE));
        DrawPaint(paint, GUILayoutUtility.GetLastRect());
        GUILayout.EndVertical();

        if (GUILayout.Button("Change Material"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.title = "Change Material";
            materialSelector.allowNullMaterial = true; // TODO: disable if no substances selected
            materialSelector.handler = voxelArray.AssignMaterial;
        }

        if (GUILayout.Button("Change Overlay"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.title = "Change Overlay";
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
        if (paint.material != null)
            GUI.DrawTexture(rect, MaterialPreview(paint.material), ScaleMode.ScaleToFit, false);
        if (paint.overlay != null)
            GUI.DrawTexture(rect, MaterialPreview(paint.overlay), ScaleMode.ScaleToFit, true);
        GUI.matrix = baseMatrix;
    }

    private Texture MaterialPreview(Material mat)
    {
        if (mat.mainTexture != null)
            return mat.mainTexture;
        else if (mat.HasProperty("_Color"))
            return ColorPreview(mat.color);
        else if (mat.HasProperty("_ColorControl"))
            // water shader
            return mat.GetTexture("_ColorControl");
        else if (mat.HasProperty("_FrontTex"))
            // skybox
            return mat.GetTexture("_FrontTex");
        return ColorPreview(Color.white);
    }

    private Texture ColorPreview(Color c)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, c);
        texture.Apply();
        return texture;
    }
}