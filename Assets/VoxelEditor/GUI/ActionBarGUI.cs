using UnityEngine;

public class ActionBarGUI : TopPanelGUI
{
    public VoxelArrayEditor voxelArray;
    public EditorFile editorFile;
    public TouchListener touchListener;

    private static readonly System.Lazy<GUIStyle> labelStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(StyleSet.buttonLarge);
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

    public override GUIStyle GetStyle() => GUIStyle.none;

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        // overflow menu will not be cut off if the buttons can't fit
        GUIUtils.BeginHorizontalClipped();

        if (ActionBarButton(IconSet.close))
            editorFile.Close();

        SelectionGUI();
        EditGUI();

        GUILayout.FlexibleSpace();

        if (ActionBarButton(IconSet.play))
            editorFile.Play();

        GUIUtils.EndHorizontalClipped();

        TutorialGUI.TutorialHighlight("bevel");
        if (ActionBarButton(IconSet.overflow))
            BuildOverflowMenu();
        TutorialGUI.ClearHighlight();

        GUILayout.EndHorizontal();
    }

    protected void SelectionGUI()
    {
        if (voxelArray.SomethingIsAddSelected())
        {
            if (ActionBarButton(IconSet.applySelection))
                voxelArray.StoreSelection();
        }

        if (voxelArray.SomethingIsStoredSelected())
        {
            if (ActionBarButton(IconSet.clearSelection))
            {
                voxelArray.ClearStoredSelection();
                voxelArray.ClearSelection();
            }
        }
    }

    protected void EditGUI(string message = null)
    {
        if (!voxelArray.SomethingIsSelected())
        {
            if (message != null)
            {
                GUILayout.FlexibleSpace();
                ActionBarLabel(message);
            }
            return;
        }
        bool facesSelected = voxelArray.FacesAreSelected();

        TutorialGUI.TutorialHighlight("paint");
        if (ActionBarButton(IconSet.paint))
        {
            PaintGUI paintGUI = gameObject.AddComponent<PaintGUI>();
            paintGUI.voxelArray = voxelArray;
            paintGUI.handler = (VoxelFace paint) =>
            {
                voxelArray.PaintSelectedFaces(paint);
            };
            paintGUI.paint = voxelArray.GetSelectedPaint();
        }
        TutorialGUI.ClearHighlight();

        TutorialGUI.TutorialHighlight("create object");
        if (facesSelected && ActionBarButton(IconSet.create))
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
                    voxelArray.PlaceObject((ObjectEntity)type.Create());
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
        else if (facesSelected)
            ActionBarLabel(SelectionString(voxelArray.selectionBounds.size));
    }

    private void BuildOverflowMenu()
    {
        var overflow = gameObject.AddComponent<OverflowMenuGUI>();
        overflow.items = new OverflowMenuGUI.MenuItem[]
        {
            new OverflowMenuGUI.MenuItem("Help", IconSet.help, () => {
                var help = gameObject.AddComponent<HelpGUI>();
                help.voxelArray = voxelArray;
                help.touchListener = touchListener;
            }),
            new OverflowMenuGUI.MenuItem("World", IconSet.world, () => {
                PropertiesGUI propsGUI = GetComponent<PropertiesGUI>();
                if (propsGUI != null)
                {
                    propsGUI.specialSelection = voxelArray.world;
                    propsGUI.normallyOpen = true;
                }
            }),
            new OverflowMenuGUI.MenuItem("Select...", IconSet.select, () => {
                var selectMenu = gameObject.AddComponent<OverflowMenuGUI>();
                selectMenu.depth = 1;
                selectMenu.items = new OverflowMenuGUI.MenuItem[]
                {
                    new OverflowMenuGUI.MenuItem("Draw", IconSet.draw, () => {
                        DrawSelectInterface();
                    }),
                    new OverflowMenuGUI.MenuItem("With Paint", IconSet.paint, () => {
                        SelectByPaintInterface();
                    }),
                    new OverflowMenuGUI.MenuItem("Fill Paint", IconSet.fill, () => {
                        FillPaintInterface();
                    }),
                    new OverflowMenuGUI.MenuItem("With Tag", IconSet.entityTag, () => {
                        SelectByTagInterface();
                    })
                };
            }, stayOpen: true),
            new OverflowMenuGUI.MenuItem("Bevel", IconSet.bevel, () => {
                var bevelGUI = gameObject.AddComponent<BevelActionBarGUI>();
                bevelGUI.voxelArray = voxelArray;
                bevelGUI.touchListener = touchListener;
            }),
            new OverflowMenuGUI.MenuItem("Revert", IconSet.undo, () => {
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

    public static bool ActionBarButton(Texture icon) =>
        GUILayout.Button(icon, StyleSet.buttonLarge, GUILayout.ExpandWidth(false));

    public static bool ActionBarButton(string text) =>
        GUILayout.Button(text, StyleSet.buttonLarge, GUILayout.ExpandWidth(false));

    public static bool HighlightedActionBarButton(Texture icon) =>
        GUIUtils.HighlightedButton(icon, StyleSet.buttonLarge, true, GUILayout.ExpandWidth(false));

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
        facePicker.onlyFaces = true;  // filling object doesn't make sense
        facePicker.clearStoredSelection = false;
        facePicker.pickAction = () =>
        {
            voxelArray.FillSelectPaint();
        };
    }

    private void DrawSelectInterface()
    {
        DrawSelectGUI drawSelect = gameObject.AddComponent<DrawSelectGUI>();
        drawSelect.voxelArray = voxelArray;
        drawSelect.touchListener = touchListener;
    }
}
