using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBarGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;
    public EditorFile editorFile;
    public TouchListener touchListener;

    private static readonly Lazy<GUIStyle> labelStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.GetStyle("button_large"));
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.background = GUI.skin.box.normal.background;
        style.border = GUI.skin.box.border;
        return style;
    });

    protected PropertiesGUI propertiesGUI;

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        propertiesGUI = GetComponent<PropertiesGUI>();

        base.OnEnable();
    }

    public override Rect GetRect(float width, float height)
    {
        return new Rect(height/2 + propertiesGUI.slide, 0,
            width - height/2 - propertiesGUI.slide, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();

        if (ActionBarButton(GUIIconSet.instance.close))
            editorFile.Close();

        SelectionGUI();
        EditGUI();

        GUILayout.FlexibleSpace();

        if (ActionBarButton(GUIIconSet.instance.play))
            editorFile.Play();

        if (ActionBarButton(GUIIconSet.instance.overflow))
        {
            var overflow = gameObject.AddComponent<OverflowMenuGUI>();
            overflow.voxelArray = voxelArray;
            overflow.touchListener = touchListener;
        }

        GUILayout.EndHorizontal();
    }

    protected void SelectionGUI()
    {
        if (voxelArray.SomethingIsAddSelected())
        {
            if (ActionBarButton(GUIIconSet.instance.applySelection))
                voxelArray.StoreSelection();
        }

        if(voxelArray.SomethingIsStoredSelected())
        {
            if (ActionBarButton(GUIIconSet.instance.clearSelection))
            {
                voxelArray.ClearStoredSelection();
                voxelArray.ClearSelection();
            }
        }
    }

    protected void EditGUI()
    {
        if (!voxelArray.FacesAreSelected())
            return;

        TutorialGUI.TutorialHighlight("paint");
        if (ActionBarButton(GUIIconSet.instance.paint))
        {
            PaintGUI paintGUI = gameObject.AddComponent<PaintGUI>();
            paintGUI.title = "Paint Faces";
            paintGUI.handler = (VoxelFace paint) =>
            {
                voxelArray.PaintSelectedFaces(paint);
            };
            paintGUI.paint = voxelArray.GetSelectedPaint();
        }
        TutorialGUI.ClearHighlight();

        TutorialGUI.TutorialHighlight("create object");
        if (ActionBarButton(GUIIconSet.instance.create))
        {
            TypePickerGUI picker = gameObject.AddComponent<TypePickerGUI>();
            picker.title = "Create";
            picker.categories = new PropertiesObjectType[][] {
                GameScripts.entityTemplates, GameScripts.objects };
            picker.categoryNames = new string[] {"Substance", "Object"};
            picker.handler = (PropertiesObjectType type) =>
            {
                if (typeof(Substance).IsAssignableFrom(type.type))
                {
                    Substance substance = (Substance)type.Create();
                    voxelArray.substanceToCreate = substance;
                    var createGUI = gameObject.AddComponent<CreateSubstanceGUI>();
                    createGUI.voxelArray = voxelArray;
                }
                else if (typeof(ObjectEntity).IsAssignableFrom(type.type))
                {
                    ObjectEntity obj = (ObjectEntity)type.Create();

                    Vector3 createPosition = voxelArray.selectionBounds.center;
                    int faceNormal = voxelArray.GetSelectedFaceNormal();
                    if (faceNormal != -1)
                        createPosition += Voxel.DirectionForFaceI(faceNormal) / 2;
                    createPosition -= new Vector3(0.5f, 0.5f, 0.5f);
                    obj.position = VoxelArray.Vector3ToInt(createPosition);

                    obj.InitObjectMarker(voxelArray);
                    voxelArray.objects.Add(obj);
                    voxelArray.unsavedChanges = true;
                    // select the object. Wait one frame so the position is correct
                    StartCoroutine(SelectNewObjectCoroutine(obj));
                }
            };
        }
        TutorialGUI.ClearHighlight();

        GUILayout.FlexibleSpace();
        int moveCount = 0;
        if (touchListener.currentTouchOperation == TouchListener.TouchOperation.MOVE)
            moveCount = Mathf.Abs(touchListener.movingAxis.moveCount);
        if (moveCount != 0)
            ActionBarLabel(moveCount.ToString());
        else
            ActionBarLabel(SelectionString(voxelArray.selectionBounds.size));
    }

    private IEnumerator SelectNewObjectCoroutine(ObjectEntity obj)
    {
        yield return null;
        voxelArray.ClearStoredSelection();
        voxelArray.TouchDown(obj.marker);
        voxelArray.TouchUp();
    }

    public static bool ActionBarButton(Texture icon)
    {
        return GUILayout.Button(icon, GUI.skin.GetStyle("button_large"), GUILayout.ExpandWidth(false));
    }

    public static bool ActionBarButton(string text)
    {
        return GUILayout.Button(text, GUI.skin.GetStyle("button_large"), GUILayout.ExpandWidth(false));
    }

    public static bool HighlightedActionBarButton(Texture icon)
    {
        return GUIUtils.HighlightedButton(icon, GUI.skin.GetStyle("button_large"), GUILayout.ExpandWidth(false));
    }

    public static void ActionBarLabel(string text)
    {
        if (text.Length == 0)
            return;
        GUILayout.Label(text, labelStyle.Value, GUILayout.ExpandWidth(false));
    }

    private string SelectionString(Vector3 selectionSize)
    {
        if (selectionSize == Vector3.zero)
            return "";
        else if (selectionSize.x == 0)
            return Mathf.RoundToInt(selectionSize.y)
                + "x" + Mathf.RoundToInt(selectionSize.z);
        else if (selectionSize.y == 0)
            return Mathf.RoundToInt(selectionSize.x)
                + "x" + Mathf.RoundToInt(selectionSize.z);
        else if (selectionSize.z == 0)
            return Mathf.RoundToInt(selectionSize.x)
                + "x" + Mathf.RoundToInt(selectionSize.y);
        else return Mathf.RoundToInt(selectionSize.x)
                + "x" + Mathf.RoundToInt(selectionSize.y)
                + "x" + Mathf.RoundToInt(selectionSize.z);
    }
}
