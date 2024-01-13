using UnityEngine;

public static class Tutorials
{
    private static GUIStringSet StringSet =>
        (GUIManager.instance != null) ? GUIManager.instance.stringSet : null;

    public static TutorialPageFactory[] INTRO_TUTORIAL = new TutorialPageFactory[]
    {
        () => new SimpleTutorialPage(StringSet.TutorialWelcome),
        () => new SimpleTutorialPage(StringSet.TutorialRoom),
        () => new TutorialIntroOrbit(),
        () => new TutorialIntroPan(),
        () => new TutorialIntroSelectFace(),
        () => new TutorialIntroPull(),
        () => new TutorialIntroPush(),
        () => new TutorialIntroColumn(),
        () => new TutorialIntroSelectBox(),
        () => new TutorialIntroSelectWall(),
        () => new SimpleTutorialPage(StringSet.TutorialSculpt),
        () => new FullScreenTutorialPage(StringSet.TutorialButtons,
            "Tutorials/toolbar_buttons", width:1536, height:798),
        () => new SimpleTutorialPage(StringSet.TutorialHelpMenu),
        () => new SimpleTutorialPage(StringSet.TutorialLinks),
    };

    public static TutorialPageFactory[] PAINT_TUTORIAL = new TutorialPageFactory[]
    {
        () => new TutorialPaintStart(),
        () => new TutorialPaintPage(StringSet.TutorialPaintPanel),
        () => new TutorialPaintPage(StringSet.TutorialPaintCategories),
        () => new TutorialPaintPage(StringSet.TutorialPaintLayers, highlight: "paint layer"),
        () => new TutorialPaintPage(StringSet.TutorialPaintTransform, highlight: "paint transform"),
        () => new TutorialPaintSky(),
    };

    public static TutorialPageFactory[] BEVEL_TUTORIAL = new TutorialPageFactory[]
    {
        () => new TutorialBevelStart(),
        () => new TutorialBevelSelect(),
        () => new TutorialBevelPage(StringSet.TutorialBevelShape, highlight: "bevel shape"),
        () => new TutorialBevelPage(StringSet.TutorialBevelSize, highlight: "bevel size"),
        () => new TutorialBevelFillSelect(),
        () => new SimpleTutorialPage(StringSet.TutorialBevelExit, highlight: "bevel done"),
    };

    public static TutorialPageFactory[] SUBSTANCE_TUTORIAL = new TutorialPageFactory[]
    {
        () => new SimpleTutorialPage(StringSet.TutorialSubstanceIntro),
        () => new SimpleTutorialPage(StringSet.TutorialSubstancePit),
        () => new TutorialSubstanceObjectCreatePanel(StringSet.TutorialSubstanceCreateButton),
        () => new TutorialSubstanceCreate1(),
        () => new TutorialSubstanceCreate2(),
        () => new TutorialSubstancePage(StringSet.TutorialSubstanceBehaviors,
            highlight: "behaviors"),
        () => new TutorialSubstanceAddBehavior(),
        () => new TutorialSubstanceEditDirection(),
        () => new TutorialSubstanceSetDirection(),
        () => new SimpleTutorialPage(StringSet.TutorialSubstancePlayTest),
        () => new TutorialSubstancePage(StringSet.TutorialSubstanceOnOff,
            highlight: "behavior condition"),
        () => new TutorialSubstanceAddOppositeBehavior(),
        () => new TutorialSubstanceBehaviorConditions(),
        () => new TutorialSubstanceAddSensor(),
        () => new TutorialSubstancePulseTime(),
        () => new SimpleTutorialPage(StringSet.TutorialSubstancePlayFinal),
        () => new SimpleTutorialPage(StringSet.TutorialSubstanceNext),
    };

    public static TutorialPageFactory[] OBJECT_TUTORIAL = new TutorialPageFactory[]
    {
        () => new TutorialSubstanceObjectCreatePanel(StringSet.TutorialObjectSelect),
        () => new TutorialObjectCreate(),
        () => new TutorialObjectPage(StringSet.TutorialObjectExplain),
        () => new TutorialObjectPaint(),
        () => new TutorialObjectAddBehavior(),
        () => new TutorialObjectFollowPlayer(),
        () => new SimpleTutorialPage(StringSet.TutorialObjectPlayTest),
        () => new TutorialObjectAddSensor(),
        () => new TutorialObjectTouchPlayer(),
        () => new TutorialObjectAddTargetedBehavior(),
        () => new TutorialObjectPage(StringSet.TutorialObjectTargetExplain),
        () => new TutorialObjectBehaviorCondition(),
        () => new TutorialObjectHurtRate(),
        () => new SimpleTutorialPage(StringSet.TutorialObjectPlayFinal),
        () => new TutorialObjectAddPhysicsBehavior(),
        () => new SimpleTutorialPage(StringSet.TutorialObjectNext),
    };

    public static TutorialPageFactory[] ADVANCED_GAME_LOGIC_TUTORIAL_1 = new TutorialPageFactory[]
    {
        () => new SimpleTutorialPage(StringSet.TutorialElevatorIntro),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorNoChecks),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorUpTap),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorToggle),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorMoveOn),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorHelpButtons),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorPlayMoveUp),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorNextFloor),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorPulse),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorPlayFloorStop),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorMoreButtons),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorTooManyButtons),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorHiddenRoom),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorUpCube),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorUpInput),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorPlayBothUpButtons),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorDownIntro),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorTargetedExplain),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorDownCube),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorRedBall),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorMoveTarget),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorMoveDown),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorPlayMoveDown),
        () => new SimpleTutorialPage(StringSet.TutorialElevatorNext),
    };

    public static TutorialPageFactory[] ADVANCED_GAME_LOGIC_TUTORIAL_2 = new TutorialPageFactory[]
    {
        () => new SimpleTutorialPage(StringSet.TutorialPitIntro),
        () => new SimpleTutorialPage(StringSet.TutorialPitBuild),
        () => new SimpleTutorialPage(StringSet.TutorialPitTrigger),
        () => new SimpleTutorialPage(StringSet.TutorialPitNoEscape),
        () => new SimpleTutorialPage(StringSet.TutorialPitHurtBehavior),
        () => new SimpleTutorialPage(StringSet.TutorialPitActivatorsExplain),
        () => new SimpleTutorialPage(StringSet.TutorialPitHurtPoints),
        () => new SimpleTutorialPage(StringSet.TutorialPitPlay),
        () => new SimpleTutorialPage(StringSet.TutorialPitBalls),
    };

    private class TutorialIntroOrbit : TutorialPage
    {
        private Quaternion startRotation;
        bool rotated;
        private float startTime;

        public override string GetText() => StringSet.TutorialOrbit;

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            startRotation = touchListener.pivot.rotation;
            startTime = Time.time;
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            if (!rotated)
            {
                var currentRotation = touchListener.pivot.rotation;
                if (Quaternion.Angle(currentRotation, startRotation) > 60)
                {
                    Debug.Log("Rotate complete");
                    rotated = true;
                }
            }
            if (rotated && Time.time - startTime > 3)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }

    private class TutorialIntroPan : TutorialPage
    {
        private Vector3 startPan;
        public override string GetText() => StringSet.TutorialPan;

        public override string GetHighlightID() => "pan";

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            startPan = touchListener.pivot.position;
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            var currentPan = touchListener.pivot.position;
            if ((currentPan - startPan).magnitude > 4)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialIntroSelectFace : TutorialPage
    {
        public override string GetText() => StringSet.TutorialSelectFace;

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            if (voxelArray.GetSelectedFaceNormal() != -1)
            {
                voxelArray.ClearSelection();
                voxelArray.ClearStoredSelection();
            }
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            if (voxelArray.GetSelectedFaceNormal() != -1)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialIntroPull : TutorialPage
    {
        private int faceNormal;

        public override string GetText() =>
            StringSet.TutorialPull(AxisNameForFaceNormal(faceNormal));

        public static string AxisNameForFaceNormal(int faceNormal)
        {
            switch (faceNormal)
            {
                case 0:
                case 1:
                    return StringSet.TutorialAxisX;
                case 2:
                case 3:
                    return StringSet.TutorialAxisY;
                case 4:
                case 5:
                    return StringSet.TutorialAxisZ;
                default:
                    return "";
            }
        }

        public static bool AxisMatchesFace(TransformAxis transformAxis, int faceNormal)
        {
            var moveAxis = transformAxis as MoveAxis;
            if (moveAxis == null)
                return false;
            if (faceNormal == -1)
                return false;
            return moveAxis.forwardDirection == Voxel.DirectionForFaceI(
                (faceNormal / 2) * 2 + 1);
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            faceNormal = voxelArray.GetSelectedFaceNormal();
            if (faceNormal == -1)
                return TutorialAction.BACK;
            if (touchListener.currentTouchOperation == TouchListener.TouchOperation.MOVE
                && AxisMatchesFace(touchListener.movingAxis, faceNormal))
            {
                int moveCount = ((MoveAxis)touchListener.movingAxis).moveCount;
                if (faceNormal % 2 == 0)
                {
                    if (moveCount <= -1)
                        return TutorialAction.NEXT;
                }
                else
                {
                    if (moveCount >= 1)
                        return TutorialAction.NEXT;
                }
            }
            return TutorialAction.NONE;
        }
    }


    private class TutorialIntroPush : TutorialPage
    {
        private Bounds startSelectedFace;
        private int faceNormal;

        public override string GetText()
        {
            string message = StringSet.TutorialPush;
            if (faceNormal != -1)
            {
                message += " " + StringSet.TutorialPushHint(
                    TutorialIntroPull.AxisNameForFaceNormal(faceNormal));
            }
            return message;
        }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            startSelectedFace = voxelArray.boxSelectStartBounds;
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (startSelectedFace == voxelArray.boxSelectStartBounds)
                faceNormal = -1; // same face selected as before
            else
                faceNormal = voxelArray.GetSelectedFaceNormal();
            if (touchListener.currentTouchOperation == TouchListener.TouchOperation.MOVE
                && TutorialIntroPull.AxisMatchesFace(touchListener.movingAxis, faceNormal))
            {
                int moveCount = ((MoveAxis)touchListener.movingAxis).moveCount;
                if (faceNormal % 2 == 0)
                {
                    if (moveCount >= 1)
                        return TutorialAction.NEXT;
                }
                else
                {
                    if (moveCount <= -1)
                        return TutorialAction.NEXT;
                }
            }
            return TutorialAction.NONE;
        }
    }


    private class TutorialIntroColumn : TutorialPage
    {
        private int lastFaceNormal = -1;

        public override string GetText() => StringSet.TutorialColumn;

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            int faceNormal = voxelArray.GetSelectedFaceNormal();
            if (faceNormal != -1)
                lastFaceNormal = faceNormal;
            if (touchListener.currentTouchOperation == TouchListener.TouchOperation.MOVE
                && TutorialIntroPull.AxisMatchesFace(touchListener.movingAxis, lastFaceNormal)
                && !voxelArray.SomethingIsSelected())
                // just made a column
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialIntroSelectBox : TutorialPage
    {
        private bool boxWasSelected;
        private int numBoxes;
        private Bounds lastStartBounds;

        public override string GetText() => StringSet.TutorialBox;

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            voxelArray.ClearSelection();
            voxelArray.ClearStoredSelection();
        }

        private bool BoxIsSelected(VoxelArrayEditor voxelArray) =>
            voxelArray.selectMode == VoxelArrayEditor.SelectMode.BOX
                && voxelArray.selectionBounds.size.sqrMagnitude > 3;

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (!boxWasSelected)
            {
                if (BoxIsSelected(voxelArray))
                {
                    Debug.Log("Selected box");
                    boxWasSelected = true;
                    numBoxes++;
                    lastStartBounds = voxelArray.boxSelectStartBounds;
                }
            }
            else
            {
                if (!voxelArray.SomethingIsSelected())
                {
                    Debug.Log("Deselected box");
                    boxWasSelected = false;
                }
                else if (voxelArray.boxSelectStartBounds != lastStartBounds)
                {
                    Debug.Log("Deselected box");
                    boxWasSelected = false;
                }
            }

            if (numBoxes >= 3)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialIntroSelectWall : TutorialPage
    {
        public override string GetText() => StringSet.TutorialWall;

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (voxelArray.selectMode == VoxelArrayEditor.SelectMode.FACE_FLOOD_FILL)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialPaintStart : TutorialPage
    {
        public override string GetText() => StringSet.TutorialPaintButton;

        public override string GetHighlightID() => "paint";

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (guiGameObject.GetComponent<PaintGUI>() != null)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialPaintPage : SimpleTutorialPage
    {
        private bool panelOpen;

        public TutorialPaintPage(string text, string highlight = "")
            : base(text, highlight) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            panelOpen = guiGameObject.GetComponent<PaintGUI>() != null;
            return TutorialAction.NONE;
        }

        public override string GetText()
        {
            if (!panelOpen)
                return StringSet.TutorialPaintReopen;
            else
                return base.GetText();
        }

        public override string GetHighlightID()
        {
            if (!panelOpen)
                return "paint";
            else
                return base.GetHighlightID();
        }

        public override bool ShowNextButton() => panelOpen;
    }


    private class TutorialPaintSky : TutorialPaintPage
    {
        public TutorialPaintSky()
            : base(StringSet.TutorialPaintSky) { }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            PaintGUI paintPanel = guiGameObject.GetComponent<PaintGUI>();
            paintPanel.TutorialShowSky();
        }
    }


    private class TutorialBevelStart : TutorialPage
    {
        public override string GetText() => StringSet.TutorialBevelButton;

        public override string GetHighlightID() => "bevel";

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (guiGameObject.GetComponent<BevelGUI>() != null)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialBevelPage : SimpleTutorialPage
    {
        private bool panelOpen;

        public TutorialBevelPage(string text, string highlight = "")
            : base(text, highlight) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            panelOpen = guiGameObject.GetComponent<BevelGUI>() != null;
            return TutorialAction.NONE;
        }

        public override string GetText()
        {
            if (!panelOpen)
                return StringSet.TutorialBevelReopen;
            else
                return base.GetText();
        }

        public override string GetHighlightID()
        {
            if (!panelOpen)
                return "bevel";
            else
                return base.GetHighlightID();
        }

        public override bool ShowNextButton() => panelOpen;
    }


    private class TutorialBevelSelect : TutorialBevelPage
    {
        public TutorialBevelSelect()
            : base(StringSet.TutorialBevelSelect) { }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            if (voxelArray.selectMode == VoxelArrayEditor.SelectMode.BOX_EDGES)
            {
                voxelArray.ClearSelection();
                voxelArray.ClearStoredSelection();
            }
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);
            if (voxelArray.selectMode == VoxelArrayEditor.SelectMode.BOX_EDGES)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialBevelFillSelect : TutorialBevelPage
    {
        public TutorialBevelFillSelect()
            : base(StringSet.TutorialBevelDoubleTap) { }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            if (voxelArray.selectMode == VoxelArrayEditor.SelectMode.EDGE_FLOOD_FILL)
            {
                voxelArray.ClearSelection();
                voxelArray.ClearStoredSelection();
            }
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);
            if (voxelArray.selectMode == VoxelArrayEditor.SelectMode.EDGE_FLOOD_FILL)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialSubstanceObjectCreatePanel : TutorialPage
    {
        private string text;

        public TutorialSubstanceObjectCreatePanel(string text)
        {
            this.text = text;
        }

        public override string GetText() => text;

        public override string GetHighlightID() => "create object";

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (guiGameObject.GetComponent<TypePickerGUI>() != null)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialSubstanceCreate1 : TutorialPage
    {
        public override string GetText() => StringSet.TutorialSubstanceSolid;

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (guiGameObject.GetComponent<CreateSubstanceGUI>() != null)
                return TutorialAction.NEXT;
            else if (guiGameObject.GetComponent<TypePickerGUI>() == null)
                return TutorialAction.BACK;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialSubstanceCreate2 : TutorialPage
    {
        public override string GetText() => StringSet.TutorialSubstancePull;

        public static bool SubstanceSelected(VoxelArrayEditor voxelArray)
        {
            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is Substance)
                    return true;
            return false;
        }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (SubstanceSelected(voxelArray))
            {
                voxelArray.ClearSelection();
                voxelArray.ClearStoredSelection();
            }
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (SubstanceSelected(voxelArray))
                return TutorialAction.NEXT;
            else if (guiGameObject.GetComponent<CreateSubstanceGUI>() == null)
                return TutorialAction.BACK;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialSubstancePage : SimpleTutorialPage
    {
        private bool substanceSelected;

        public TutorialSubstancePage(string text, string highlight = "")
            : base(text, highlight) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            substanceSelected = TutorialSubstanceCreate2.SubstanceSelected(voxelArray)
                || guiGameObject.GetComponent<EntityPickerGUI>() != null;
            return TutorialAction.NONE;
        }

        public override string GetText()
        {
            if (!substanceSelected)
                return StringSet.TutorialSubstanceReselect;
            else
                return base.GetText();
        }

        public override bool ShowNextButton() => substanceSelected;
    }


    private class TutorialSubstanceAddBehavior : TutorialSubstancePage
    {
        public TutorialSubstanceAddBehavior()
            : base(StringSet.TutorialSubstanceMoveBehavior, highlight: "add behavior") { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is Substance)
                    foreach (EntityBehavior behavior in e.behaviors)
                        if (behavior is MoveBehavior)
                            return TutorialAction.NEXT;
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialSubstanceEditDirection : TutorialSubstancePage
    {
        public TutorialSubstanceEditDirection()
            : base(StringSet.TutorialSubstanceEditDirection) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            if (guiGameObject.GetComponent<TargetGUI>() != null)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialSubstanceSetDirection : TutorialSubstancePage
    {
        private bool panelWasOpen;

        public TutorialSubstanceSetDirection()
            : base(StringSet.TutorialSubstanceSetDirection) { }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Start(voxelArray, guiGameObject, touchListener);
            panelWasOpen = guiGameObject.GetComponent<TargetGUI>() != null;
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            if (!panelWasOpen)
                return TutorialAction.BACK;
            else if (guiGameObject.GetComponent<TargetGUI>() != null)
                return TutorialAction.NONE;
            else
                return TutorialAction.NEXT;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialSubstanceAddOppositeBehavior : TutorialSubstancePage
    {
        public TutorialSubstanceAddOppositeBehavior()
            : base(StringSet.TutorialSubstanceMoveOpposite) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            // check if there are two move behaviors going the opposite direction
            foreach (Entity e in voxelArray.GetSelectedEntities())
            {
                if (e is Substance)
                {
                    int direction = -1;

                    foreach (EntityBehavior behavior in e.behaviors)
                    {
                        if (behavior is MoveBehavior)
                        {
                            if (direction == -1)
                            {
                                direction = GetMoveBehaviorDirection(behavior);
                                if (direction == -1)
                                    return TutorialAction.NONE;
                            }
                            else
                            {
                                var behaviorDirection = GetMoveBehaviorDirection(behavior);
                                if (behaviorDirection == Voxel.OppositeFaceI(direction))
                                    return TutorialAction.NEXT;
                                else
                                    return TutorialAction.NONE;
                            }
                        }
                    }
                }
            }
            return TutorialAction.NONE;
        }

        private sbyte GetMoveBehaviorDirection(EntityBehavior behavior)
        {
            object value = PropertiesObjectType.GetProperty(behavior, "dir");
            if (value == null)
                return -1;
            else
                return ((Target)value).direction;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialSubstanceBehaviorConditions : TutorialSubstancePage
    {
        public TutorialSubstanceBehaviorConditions()
            : base(StringSet.TutorialSubstanceBehaviorConditions, highlight: "behavior condition")
        { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            bool foundOff = false, foundOn = false;

            foreach (Entity e in voxelArray.GetSelectedEntities())
            {
                if (e is Substance)
                {
                    foreach (EntityBehavior behavior in e.behaviors)
                    {
                        if (behavior is MoveBehavior)
                        {
                            if (behavior.condition == EntityBehavior.Condition.OFF)
                                foundOff = true;
                            if (behavior.condition == EntityBehavior.Condition.ON)
                                foundOn = true;
                        }
                    }
                }
            }
            return foundOff && foundOn ? TutorialAction.NEXT : TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialSubstanceAddSensor : TutorialSubstancePage
    {
        public TutorialSubstanceAddSensor()
            : base(StringSet.TutorialSubstanceSensor, highlight: "change sensor") { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is Substance && e.sensor is PulseSensor)
                    return TutorialAction.NEXT;
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialSubstancePulseTime : TutorialSubstancePage
    {
        public TutorialSubstancePulseTime()
            : base(StringSet.TutorialSubstanceTime) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            bool onTimeSet = false, offTimeSet = false;
            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is Substance && e.sensor is PulseSensor)
                {
                    float offTime = (float)(PropertiesObjectType.GetProperty(e.sensor, "oft"));
                    if (offTime > 1)
                        offTimeSet = true;
                    float onTime = (float)(PropertiesObjectType.GetProperty(e.sensor, "ont"));
                    if (onTime > 1)
                        onTimeSet = true;
                }
            return onTimeSet && offTimeSet ? TutorialAction.NEXT : TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialObjectCreate : TutorialPage
    {
        public override string GetText() => StringSet.TutorialObjectCreate;

        public static bool ObjectSelected(VoxelArrayEditor voxelArray)
        {
            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is BallObject)
                    return true;
            return false;
        }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (ObjectSelected(voxelArray))
            {
                voxelArray.ClearSelection();
                voxelArray.ClearStoredSelection();
            }
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (ObjectSelected(voxelArray))
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialObjectPage : SimpleTutorialPage
    {
        private bool objectSelected;

        public TutorialObjectPage(string text, string highlight = "")
            : base(text, highlight) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            objectSelected = TutorialObjectCreate.ObjectSelected(voxelArray)
                || guiGameObject.GetComponent<EntityPickerGUI>() != null;
            return TutorialAction.NONE;
        }

        public override string GetText()
        {
            if (!objectSelected)
                return StringSet.TutorialObjectReselect;
            else
                return base.GetText();
        }

        public override bool ShowNextButton() => objectSelected;
    }


    private class TutorialObjectPaint : TutorialObjectPage
    {
        private Material prevMat = null;

        public TutorialObjectPaint()
            : base(StringSet.TutorialObjectPaint, highlight: "paint") { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is BallObject)
                {
                    Material mat = ((BallObject)e).paint.material;
                    if (prevMat == null)
                        prevMat = mat;
                    else if (prevMat != mat)
                        return TutorialAction.NEXT;
                }
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialObjectAddBehavior : TutorialObjectPage
    {
        public TutorialObjectAddBehavior()
            : base(StringSet.TutorialObjectAddMove, highlight: "add behavior") { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is BallObject)
                    foreach (EntityBehavior behavior in e.behaviors)
                        if (behavior is MoveBehavior)
                            return TutorialAction.NEXT;
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialObjectFollowPlayer : TutorialObjectPage
    {
        public TutorialObjectFollowPlayer()
            : base(StringSet.TutorialObjectFollowPlayer) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is BallObject)
                    foreach (EntityBehavior behavior in e.behaviors)
                        if (behavior is MoveBehavior)
                        {
                            Target moveTarget = (Target)PropertiesObjectType.GetProperty(behavior, "dir");
                            if (moveTarget.entityRef.entity is PlayerObject)
                                return TutorialAction.NEXT;
                        }
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialObjectAddSensor : TutorialObjectPage
    {
        public TutorialObjectAddSensor()
            : base(StringSet.TutorialObjectSensor, highlight: "change sensor") { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is BallObject && e.sensor is TouchSensor)
                    return TutorialAction.NEXT;
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialObjectTouchPlayer : TutorialObjectPage
    {
        public TutorialObjectTouchPlayer()
            : base(StringSet.TutorialObjectTouchPlayer) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is BallObject && e.sensor is TouchSensor)
                {
                    ActivatedSensor.Filter touchFilter =
                        (ActivatedSensor.Filter)PropertiesObjectType.GetProperty(e.sensor, "fil");
                    if (touchFilter is ActivatedSensor.EntityFilter
                            && ((ActivatedSensor.EntityFilter)touchFilter).entityRef.entity is PlayerObject)
                        return TutorialAction.NEXT;
                }
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialObjectAddTargetedBehavior : TutorialObjectPage
    {
        public TutorialObjectAddTargetedBehavior()
            : base(StringSet.TutorialObjectAddTargetedBehavior, highlight: "behavior target") { }

        private bool incorrectTarget = false;

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            bool hasHurtBehavior = false;
            incorrectTarget = false;
            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is BallObject)
                    foreach (EntityBehavior behavior in e.behaviors)
                        if (behavior is HurtHealBehavior)
                        {
                            hasHurtBehavior = true;
                            if (!(behavior.targetEntity.entity is PlayerObject))
                                incorrectTarget = true;
                        }
            if (hasHurtBehavior && !incorrectTarget)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }

        public override string GetText()
        {
            if (incorrectTarget)
                return StringSet.TutorialObjectIncorrectTarget;
            else
                return base.GetText();
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialObjectBehaviorCondition : TutorialObjectPage
    {
        public TutorialObjectBehaviorCondition()
            : base(StringSet.TutorialObjectHurtOn) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is BallObject)
                    foreach (EntityBehavior behavior in e.behaviors)
                        if (behavior is HurtHealBehavior && behavior.condition == EntityBehavior.Condition.ON)
                            return TutorialAction.NEXT;
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialObjectHurtRate : TutorialObjectPage
    {
        public TutorialObjectHurtRate()
            : base(StringSet.TutorialObjectHurtRate) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is BallObject)
                    foreach (EntityBehavior behavior in e.behaviors)
                        if (behavior is HurtHealBehavior)
                        {
                            float rate = (float)PropertiesObjectType.GetProperty(behavior, "rat");
                            if (rate != 0)
                                return TutorialAction.NEXT;
                        }
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }


    private class TutorialObjectAddPhysicsBehavior : TutorialObjectPage
    {
        public TutorialObjectAddPhysicsBehavior()
            : base(StringSet.TutorialObjectCharacterBehavior) { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is BallObject)
                    foreach (EntityBehavior behavior in e.behaviors)
                        if (behavior is CharacterBehavior)
                            return TutorialAction.NEXT;
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton() => false;
    }
}