using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBarGUI : TopPanelGUI
{
    public const string OBJECT_NO_ROOM_ERROR = "There's no room to put an object there!";

    public VoxelArrayEditor voxelArray;
    public EditorFile editorFile;
    public TouchListener touchListener;

    private static readonly System.Lazy<GUIStyle> labelStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUIStyleSet.instance.buttonLarge);
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.background = GUI.skin.box.normal.background;
        style.border = GUI.skin.box.border;
        return style;
    });

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;

        base.OnEnable();
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        // overflow menu will not be cut off if the buttons can't fit
        GUILayout.BeginScrollView(Vector2.zero, GUIStyle.none);
        GUILayout.BeginHorizontal();

        if (ActionBarButton(GUIIconSet.instance.close))
            editorFile.Close();

        SelectionGUI();
        EditGUI();

        GUILayout.FlexibleSpace();

        if (ActionBarButton(GUIIconSet.instance.play))
            editorFile.Play();

        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();

        TutorialGUI.TutorialHighlight("bevel");
        if (ActionBarButton(GUIIconSet.instance.overflow))
            BuildOverflowMenu();
        TutorialGUI.ClearHighlight();

        GUILayout.EndHorizontal();
    }

    protected void SelectionGUI()
    {
        if (voxelArray.SomethingIsAddSelected())
        {
            if (ActionBarButton(GUIIconSet.instance.applySelection))
                voxelArray.StoreSelection();
        }

        if (voxelArray.SomethingIsStoredSelected())
        {
            if (ActionBarButton(GUIIconSet.instance.clearSelection))
            {
                voxelArray.ClearStoredSelection();
                voxelArray.ClearSelection();
            }
        }
    }

    protected void EditGUI(string message = null)
    {
        if (!voxelArray.FacesAreSelected())
        {
            if (message != null)
            {
                GUILayout.FlexibleSpace();
                ActionBarLabel(message);
            }
            return;
        }

        TutorialGUI.TutorialHighlight("paint");
        if (ActionBarButton(GUIIconSet.instance.paint))
        {
            PaintGUI paintGUI = gameObject.AddComponent<PaintGUI>();
            paintGUI.title = "Paint Faces";
            paintGUI.voxelArray = voxelArray;
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
                GameScripts.entityTemplates, GameScripts.objectTemplates };
            picker.categoryNames = new string[] { "Substance", "Object" };
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
                    if (!voxelArray.PlaceObject(obj))
                        DialogGUI.ShowMessageDialog(gameObject, OBJECT_NO_ROOM_ERROR);
                }
            };
        }
        TutorialGUI.ClearHighlight();

        GUILayout.FlexibleSpace();
        int moveCount = 0;
        if (touchListener.currentTouchOperation == TouchListener.TouchOperation.MOVE
            && touchListener.movingAxis is MoveAxis)
            moveCount = Mathf.Abs(((MoveAxis)touchListener.movingAxis).moveCount);
        if (moveCount != 0)
            ActionBarLabel(moveCount.ToString());
        else if (message != null)
            ActionBarLabel(message);
        else
            ActionBarLabel(SelectionString(voxelArray.selectionBounds.size));
    }

    private void BuildOverflowMenu()
    {
        var overflow = gameObject.AddComponent<OverflowMenuGUI>();
        overflow.items = new OverflowMenuGUI.MenuItem[]
        {
            new OverflowMenuGUI.MenuItem("Help", GUIIconSet.instance.help, () => {
                var help = gameObject.AddComponent<HelpGUI>();
                help.voxelArray = voxelArray;
                help.touchListener = touchListener;
            }),
            new OverflowMenuGUI.MenuItem("World", GUIIconSet.instance.world, () => {
                PropertiesGUI propsGUI = GetComponent<PropertiesGUI>();
                if (propsGUI != null)
                {
                    propsGUI.specialSelection = voxelArray.world;
                    propsGUI.normallyOpen = true;
                }
            }),
            new OverflowMenuGUI.MenuItem("Select...", GUIIconSet.instance.select, () => {
                var selectMenu = gameObject.AddComponent<OverflowMenuGUI>();
                selectMenu.depth = 1;
                selectMenu.items = new OverflowMenuGUI.MenuItem[]
                {
                    new OverflowMenuGUI.MenuItem("Draw", GUIIconSet.instance.draw, () => {
                        DrawSelectInterface();
                    }),
                    new OverflowMenuGUI.MenuItem("With Paint", GUIIconSet.instance.paint, () => {
                        SelectByPaintInterface();
                    }),
                    new OverflowMenuGUI.MenuItem("Fill Paint", GUIIconSet.instance.fill, () => {
                        FillPaintInterface();
                    }),
                    new OverflowMenuGUI.MenuItem("With Tag", GUIIconSet.instance.entityTag, () => {
                        SelectByTagInterface();
                    })
                };
            }, stayOpen: true),
            new OverflowMenuGUI.MenuItem("Bevel", GUIIconSet.instance.bevel, () => {
                var bevelGUI = gameObject.AddComponent<BevelActionBarGUI>();
                bevelGUI.voxelArray = voxelArray;
                bevelGUI.touchListener = touchListener;
            }),
            new OverflowMenuGUI.MenuItem("Revert", GUIIconSet.instance.undo, () => {
                var dialog = gameObject.AddComponent<DialogGUI>();
                dialog.title = "Are you sure?";
                dialog.message = "Undo all changes since the world was opened?";
                dialog.yesButtonText = "Yes";
                dialog.noButtonText = "No";
                dialog.yesButtonHandler = () => {
                    editorFile.Revert();
                };
            })
        };
    }

    public static bool ActionBarButton(Texture icon)
    {
        return GUILayout.Button(icon, GUIStyleSet.instance.buttonLarge, GUILayout.ExpandWidth(false));
    }

    public static bool ActionBarButton(string text)
    {
        return GUILayout.Button(text, GUIStyleSet.instance.buttonLarge, GUILayout.ExpandWidth(false));
    }

    public static bool HighlightedActionBarButton(Texture icon)
    {
        return GUIUtils.HighlightedButton(icon, GUIStyleSet.instance.buttonLarge, true, GUILayout.ExpandWidth(false));
    }

    public static void ActionBarLabel(string text)
    {
        if (text.Length == 0)
            return;
        GUILayout.Label(text, labelStyle.Value, GUILayout.ExpandWidth(false));
    }

    protected string SelectionString(Vector3 selectionSize)
    {
        string selectionString = "";
        if (selectionSize.x != 0)
        {
            if (selectionString != "")
                selectionString += "x";
            selectionString += Mathf.RoundToInt(selectionSize.x);
        }
        if (selectionSize.y != 0)
        {
            if (selectionString != "")
                selectionString += "x";
            selectionString += Mathf.RoundToInt(selectionSize.y);
        }
        if (selectionSize.z != 0)
        {
            if (selectionString != "")
                selectionString += "x";
            selectionString += Mathf.RoundToInt(selectionSize.z);
        }
        return selectionString;
    }


    private void SelectByTagInterface()
    {
        TagPickerGUI tagPicker = gameObject.AddComponent<TagPickerGUI>();
        tagPicker.title = "Select by tag";
        tagPicker.handler = (byte tag) =>
        {
            voxelArray.ClearSelection();
            voxelArray.SelectAllWithTag(tag);
        };
    }

    private void SelectByPaintInterface()
    {
        FacePickerGUI facePicker = gameObject.AddComponent<FacePickerGUI>();
        facePicker.voxelArray = voxelArray;
        facePicker.message = "Tap to pick paint...";
        facePicker.onlyFaces = true;
        facePicker.clearStoredSelection = false;
        facePicker.pickAction = () =>
        {
            VoxelFace paint = voxelArray.GetSelectedPaint();
            voxelArray.ClearSelection();
            if (paint.IsEmpty())
                return;
            voxelArray.SelectAllWithPaint(paint);
        };
    }

    private void FillPaintInterface()
    {
        FacePickerGUI facePicker = gameObject.AddComponent<FacePickerGUI>();
        facePicker.voxelArray = voxelArray;
        facePicker.message = "Tap to fill paint...";
        facePicker.onlyFaces = true;
        facePicker.clearStoredSelection = false;
        facePicker.pickAction = () =>
        {
            voxelArray.FillSelectPaint();
        };
    }

    private void DrawSelectInterface()
    {
        DrawSelectGUI drawSelect= gameObject.AddComponent<DrawSelectGUI>();
        drawSelect.voxelArray = voxelArray;
        drawSelect.touchListener = touchListener;
    }
}
