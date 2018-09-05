using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorials
{
    public static TutorialPageFactory[] INTRO_TUTORIAL = new TutorialPageFactory[]
    {
        () => new SimpleTutorialPage(
            "Welcome! This is a brief tutorial that will guide you through the app. "
            + "You can access this tutorial and others at any time. Press the right arrow to continue."),
        () => new SimpleTutorialPage(
            "Right now you are looking at the interior of a room. One wall is hidden so you can see inside. "
            + "The green person marks the player location."),
        () => new TutorialIntroNavigation(),
        () => new TutorialIntroSelectFace(),
        () => new TutorialIntroPull(),
        () => new TutorialIntroPush(),
        () => new TutorialIntroColumn(),
        () => new TutorialIntroSelectBox(),
        () => new TutorialIntroSelectWall(),
        () => new SimpleTutorialPage(
            "By selecting faces and pushing/pulling them, you can sculpt the world."),
        () => new FullScreenTutorialPage(
            "These buttons appear at the top of the screen, based on context.",
            "Tutorials/toolbar_buttons", scale: 1.5f),
        () => new SimpleTutorialPage(
            "Good luck! You can access more tutorials by choosing Help in the menu.")
    };

    public static TutorialPageFactory[] PAINT_TUTORIAL = new TutorialPageFactory[]
    {
        () => new TutorialPaintStart(),
        () => new TutorialPaintPage(
            "You can use the Paint panel to paint the selected faces with <i>materials</i> and <i>overlays</i>."),
        () => new TutorialPaintPage(
            "Choose any of the categories in the list to browse textures. Or switch to the Color tab to paint a solid color.",
            highlight: "material type"),
        () => new TutorialPaintPage(
            "A paint is composed of two parts: an opaque material and a transparent overlay. "
            + "Use the tabs to switch between the two parts.",
            highlight: "paint layer"),
        () => new TutorialPaintPage(
            "Use these buttons to rotate and mirror the paint.",
            highlight: "paint transform"),
        () => new TutorialPaintSky()
    };

    public static TutorialPageFactory[] OLD_SUBSTANCE_TUTORIAL = new TutorialPageFactory[]
    {
        () => new TutorialSubstanceStart(),
        () => new TutorialSubstanceCreate(),
        () => new TutorialSubstancePage(
            "This is a <i>Substance</i>. Unlike the static blocks forming the world, "
            + "substances are independent objects that move and respond to interaction."),
        () => new TutorialSubstancePage(
            "Substances are controlled by their <i>Behaviors</i>. "
            + "This substance has <i>Visible</i> and <i>Solid</i> behaviors which make it visible and solid in the game.",
            highlight: "behaviors"),
        () => new TutorialSubstanceAddBehavior(),
        () => new TutorialSubstanceFollowPlayer(),
        () => new TutorialSubstanceAddSensor(),
        () => new TutorialSubstanceTouchPlayer(),
        () => new TutorialSubstancePage(
            "To make the sensor have an effect, behaviors can be set to be active only when the sensor is On or Off. ",
            highlight: "behavior condition"),
        () => new TutorialSubstanceHurt(),
        () => new SimpleTutorialPage(
            "<i>Try your game. What happens when you touch the substance?</i>"),
        () => new SimpleTutorialPage(
            "Read <i>Advanced Game Logic</i> in the Help menu to learn how to add more complex interactivity to games.")
    };

    public static TutorialPageFactory[] SUBSTANCE_TUTORIAL = new TutorialPageFactory[]
    {
        () => new SimpleTutorialPage(
            "In this tutorial you will build a moving platform using a <i>Substance.</i> "
            + "Substances are independent objects that can move and respond to interaction."),
        () => new SimpleTutorialPage(
            "First build a pit that is too wide and deep to cross by jumping."),
        () => new SimpleTutorialPage(
            "Now we'll add a substance which will become a moving platform. "
            + "<i>Select a row of faces on one side of the pit and tap the cube button.</i>"),
        () => new SimpleTutorialPage(
            "<i>Choose \"Solid Substance\".</i>"),
        () => new SimpleTutorialPage(
            "<i>Pull outwards to build a platform.</i>"),
        () => new SimpleTutorialPage(
            "Substances are controlled by their <i>Behaviors</i>. "
            + "This substance has <i>Visible</i> and <i>Solid</i> behaviors which make it visible and solid in the game."),
        () => new SimpleTutorialPage(
            "<i>Try adding a Move behavior to the substance.</i> Notice that behaviors are organized into multiple categories."),
        () => new SimpleTutorialPage(
            "The Move behavior will make this substance West at a constant rate. "
            + "<i>Tap the direction to edit it.</i>"),
        () => new SimpleTutorialPage(
            "<i>Make it move toward the other side of the pit (look at the compass arrow for guidance)</i>"),
        () => new SimpleTutorialPage(
            "<i>Try playing your game.</i> The platform should move continuously in one direction. "
            + "We need to make it change directions at the ends of the pit."),
        () => new SimpleTutorialPage(
            "Substances have two states, On and Off. Behaviors can be configured to only be active in the On or Off state."),
        () => new SimpleTutorialPage(
            "<i>Add a second Move behavior, which moves the platform in the opposite direction.</i>"),
        () => new SimpleTutorialPage(
            "<i>Now make one behavior active only in the Off state, and one in the On state.</i> (the substance will start Off)."),
        () => new SimpleTutorialPage(
            "A substance's On/Off state is controlled by a Sensor. "
            + "<i>Add a Pulse sensor to the platform.</i> Read its description in the list to learn its function."),
        () => new SimpleTutorialPage(
            "<i>Now adjust the time the sensor spends in the on and off state "
            + "to make the platform move the full distance of the pit.</i>"),
        () => new SimpleTutorialPage(
            "<i>Play your game now.</i> "
            + "If you built everything correctly, the platform should move across the pit and back repeatedly."),
        () => new SimpleTutorialPage(
            "Next try the <i>Objects</i> tutorial to learn about another type of interactive element.")
    };

    public static TutorialPageFactory[] OBJECT_TUTORIAL = new TutorialPageFactory[]
    {
        () => new SimpleTutorialPage(
            "<i>Select a face and tap the cube button.</i>"),
        () => new SimpleTutorialPage(
            "<i>Choose the Object tab, then choose Ball.</i>"),
        () => new SimpleTutorialPage(
            "You have just created a ball Object. "
            + "Like substances, you can give Objects behaviors and sensors to add interactivity."),
        () => new SimpleTutorialPage(
            "<i>Try changing the color of the ball.</i> (you will need to deselect it to see the effects)"),
        () => new SimpleTutorialPage(
            "<i>Add a Move behavior to the ball.</i>"),
        () => new SimpleTutorialPage(
            "<i>Edit the Move behavior to make the ball follow the player.</i>"),
        () => new SimpleTutorialPage(
            "<i>Try playing your game.</i> Next we are going to make the ball hurt you when you touch it."),
        () => new SimpleTutorialPage(
            "<i>Give the ball a Touch sensor.</i>"),
        () => new SimpleTutorialPage(
            "<i>Now configure the touch sensor so it only turns on when touching the player.</i>"),
        () => new SimpleTutorialPage(
            "To make the ball hurt the player, we'll use a \"Hurt/Heal\" behavior. "
            + "Normally this would hurt the ball, but we can specify a Target object to act upon instead."),
        () => new SimpleTutorialPage(
            "<i>Tap Add Behavior. "
            + "In the behavior menu, tap the \"Target\" button and select the player as the target. "
            + "Then select the Hurt/Heal behavior.</i>"),
        () => new SimpleTutorialPage(
            "<i>Set the Hurt/Heal behavior to activate when the Sensor is On. "
            + "Also set the rate to 1 to make it hurt continuously as long as you're touching the ball.</i>"),
        () => new SimpleTutorialPage(
            "<i>Play your game, and try to avoid dying!</i> "
            + "You can change the speed of the ball and the hurt amount to adjust the difficulty."),
        () => new SimpleTutorialPage(
            "If you add obstacles to your game, you'll notice that the ball can move through walls. "
            + "<i>Give it a Physics behavior to fix this.</i> (it's in a different tab in the list)"),
        () => new SimpleTutorialPage(
            "Read the <i>Advanced Game Logic</i> tutorial to learn how to add more complex interactivity to games.")
    };

    public const string TIPS_AND_SHORTCUTS_TUTORIAL =
@"•  Double tap to select an entire wall. The selection will be bounded by already-selected faces.
•  Triple tap a face to select <i>all</i> faces connected to it. The selection will be bounded by already-selected faces.
•  Triple tap a substance to select the entire substance.
•  Check the ""X-ray"" box of a substance to make it transparent in the editor only. This lets you see behind it and zoom through it.
•  The paint panel keeps shortcuts for the four most recent paints. To ""copy"" a paint to another face, select the origin face, open and close the paint panel, then select the destination faces and use the recent paint shortcut.
•  Sliding faces sideways along a wall moves their paints, leaving a trail behind them.
";

    public static TutorialPageFactory[] ADVANCED_GAME_LOGIC_TUTORIAL_1 = new TutorialPageFactory[]
    {
        () => new SimpleTutorialPage(
            "Here are three floors with an elevator connecting them. "
            + "Your task is to make the elevator move when you press the buttons."),
        () => new SimpleTutorialPage(
            "We’ll start by making the Up button on the first floor work. <i>Give it a Tap sensor.</i>"),
        () => new SimpleTutorialPage(
            "<i>Now give the elevator a Toggle sensor, and connect the On input to the Up button.</i>"),
        () => new SimpleTutorialPage(
            "<i>Finally, use a Move behavior to make the elevator go up only when its sensor is On.</i>"),
        () => new SimpleTutorialPage(
            "To learn more about these sensors and behaviors, you can tap their icons in the Properties panel."),
        () => new SimpleTutorialPage(
            "<i>Play your game.</i> The elevator should move up when you press the button. Why does it do this?"),
        () => new SimpleTutorialPage(
            "The elevator needs to stop when it reaches the next floor. There are multiple ways to do this. "
            + "One way is to make it turn off after 5 seconds..."),
        () => new SimpleTutorialPage(
            "<i>Find the hidden room near the elevator shaft.</i> "
            + "This room will never be seen in the game, so it's a good place to hide extra logic components."),
        () => new SimpleTutorialPage(
            "<i>Select the green cube. Give it a Delay sensor. "
            + "Connect the input to the elevator and change the On time to 5.</i>"),
        () => new SimpleTutorialPage(
            "<i>Now select the elevator, and connect its Off input to the green cube.</i>"),
        () => new SimpleTutorialPage(
            "<i>Play your game.</i> The elevator should stop on the second floor. Why does it do this?"),
        () => new SimpleTutorialPage(
            "We have one functioning button, but there are still three more. <i>Give them all Tap behaviors.</i> "
            + "We'll make the second Up button work next..."),
        () => new SimpleTutorialPage(
            "The elevator’s Toggle sensor only allows one input, but we have two Up buttons."
            + " We need to merge them into a single input..."),
        () => new SimpleTutorialPage(
            "<i>In the hidden room, select the Up arrow cube. "
            + "Give it a Threshold sensor, and connect two inputs to both of the Up buttons.</i>"),
        () => new SimpleTutorialPage(
            "<i>Now select the elevator. Change its On input to the Up arrow cube in the hidden room.</i>"),
        () => new SimpleTutorialPage(
            "<i>Play your game.</i> Both Up arrows should function correctly. Why does this work?"),
        () => new SimpleTutorialPage(
            "Now the elevator needs to go down. This makes 3 possible states: "
            + "going up, going down, and stopped. But sensors can only be On/Off..."),
        () => new SimpleTutorialPage(
            "To solve this, we use Targeted Behaviors. "
            + "Remember that these are behaviors which use their host object to turn on/off, but act upon a Target object."),
        () => new SimpleTutorialPage(
            "<i>First, connect the hidden Down arrow cube to the two Down buttons, just like the Up arrow cube.</i>"),
        () => new SimpleTutorialPage(
            "<i>Next, select the blue cube. Give it a Toggle sensor and connect the On input to the Down arrow cube.</i>"),
        () => new SimpleTutorialPage(
            "<i>Now tap Add Behavior. In the behavior menu, tap the \"Target\" button and select the elevator as the target. "
            + "Then select the Move behavior.</i>"),
        () => new SimpleTutorialPage(
            "<i>Make the move behavior go Down only when the sensor is On.</i> "
            + "The sensor of the blue cube will turn it on/off, but the Elevator will move!"),
        () => new SimpleTutorialPage(
            "<i>Play the game and try the Down button.</i> The elevator should go down, but not stop when it gets to the bottom."),
        () => new SimpleTutorialPage(
            "The last step is to stop the elevator after going down one floor. See if you can figure out how, using the red cube.")
    };

    public static TutorialPageFactory[] ADVANCED_GAME_LOGIC_TUTORIAL_2 = new TutorialPageFactory[]
    {
        () => new SimpleTutorialPage(
            "Your task is to build a Pit of Death. Anything that falls in the pit will die."),
        () => new SimpleTutorialPage(
            "<i>Make a large room with a pit in the middle.</i> Make sure the pit isn't directly under the player."),
        () => new SimpleTutorialPage(
            "<i>Add a Trigger substance spanning the bottom of the pit.</i>"),
        () => new SimpleTutorialPage(
            "The trigger should already have a Touch sensor with the filter set to Anything. "
            + "This is good because nothing escapes the Pit of Death."),
        () => new SimpleTutorialPage(
            "<i>Tap Add Behavior. Tap the Target button, then tap Activators. Then choose the Hurt/Heal behavior.</i>"),
        () => new SimpleTutorialPage(
            "This behavior targets its \"Activators\", which are the objects that \"cause\" the sensor to turn on -- "
            + "in this case, any objects that touch the trigger."),
        () => new SimpleTutorialPage(
            "<i>Make the trigger hurt the Activator by -100 points.</i>"),
        () => new SimpleTutorialPage(
            "<i>Play your game. Try jumping in the pit.</i>"),
        () => new SimpleTutorialPage(
            "<i>Now try making some solid substances with Physics behaviors. "
            + "Then play the game and push them all into the pit.</i> Enjoy."),
    };

    private class TutorialIntroNavigation : TutorialPage
    {
        private Quaternion startRotation;
        private Vector3 startPan;
        private bool rotate, pan;
        private float startTime;

        public override string GetText()
        {
            return "Navigation: Use two fingers to rotate and zoom, and three fingers to pan. "
                + "<i>Try looking around the room.</i>";
        }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            startRotation = touchListener.pivot.rotation;
            startPan = touchListener.pivot.position;
            startTime = Time.time;
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            if (!rotate)
            {
                var currentRotation = touchListener.pivot.rotation;
                if (Quaternion.Angle(currentRotation, startRotation) > 60)
                {
                    Debug.Log("Rotate complete");
                    rotate = true;
                }
            }
            if (!pan)
            {
                var currentPan = touchListener.pivot.position;
                if ((currentPan - startPan).magnitude > 4)
                {
                    Debug.Log("Pan complete");
                    pan = true;
                }
            }
            if (rotate && pan && Time.time - startTime > 4)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialIntroSelectFace : TutorialPage
    {
        public override string GetText()
        {
            return "<i>Tap with one finger to select a single face of a block.</i>";
        }

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

        public override string GetText()
        {
            string axisName = AxisNameForFaceNormal(faceNormal);
            return "<i>Pull the " + axisName + " arrow towards the center of the room to pull the block out.</i>";
        }

        public static string AxisNameForFaceNormal(int faceNormal)
        {
            switch (faceNormal)
            {
                case 0:
                case 1:
                    return "Red";
                case 2:
                case 3:
                    return "Green";
                case 4:
                case 5:
                    return "Blue";
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
            string message = "<i>Now select a different face and push it away from the center of the room.</i>";
            if (faceNormal != -1)
                message += " <i>(hint: use the " + TutorialIntroPull.AxisNameForFaceNormal(faceNormal)
                    + " arrow)</i>";
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

        public override string GetText()
        {
            return "<i>Now select a different face and pull it towards the center of the room. "
                + "Keep pulling as far as you can.</i>";
        }

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

        public override string GetText()
        {
            return "Tap and drag to select a group of faces in a rectangle or box. <i>Try this a few times.</i>";
        }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            voxelArray.ClearSelection();
            voxelArray.ClearStoredSelection();
        }

        private bool BoxIsSelected(VoxelArrayEditor voxelArray)
        {
            return voxelArray.selectMode == VoxelArrayEditor.SelectMode.BOX
                && voxelArray.selectionBounds.size.sqrMagnitude > 3;
        }

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
        public override string GetText()
        {
            return "<i>Double tap to select an entire wall.</i>";
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (voxelArray.selectMode == VoxelArrayEditor.SelectMode.FACE)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialPaintStart : TutorialPage
    {
        public override string GetText()
        {
            return "<i>Select a face and tap the paint roller icon to open the Paint panel.</i>";
        }

        public override string GetHighlightID()
        {
            return "paint";
        }

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
                return "<i>Reopen the paint panel. (Select a face and tap the paint roller icon)</i>";
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

        public override bool ShowNextButton()
        {
            return panelOpen;
        }
    }


    private class TutorialPaintSky : TutorialPaintPage
    {
        public TutorialPaintSky()
            : base("The \"Sky\" material is special: in the game it is an unobstructed window to the sky. "
                + "Since the world can't have holes, this is the only way to see the sky.") { }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            PaintGUI paintPanel = guiGameObject.GetComponent<PaintGUI>();
            paintPanel.TutorialShowSky();
        }
    }


    private class TutorialSubstanceStart : TutorialPage
    {
        public override string GetText()
        {
            return "<i>Select a face and tap the cube button.</i>";
        }

        public override string GetHighlightID()
        {
            return "create object";
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (guiGameObject.GetComponent<TypePickerGUI>() != null)
                return TutorialAction.NEXT;
            else
                return TutorialAction.NONE;
        }
    }


    private class TutorialSubstanceCreate : TutorialPage
    {
        public override string GetText()
        {
            return "<i>Choose \"Solid Substance\" and follow the instructions.</i>";
        }

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
            else if (guiGameObject.GetComponent<TypePickerGUI>() == null
                    && guiGameObject.GetComponent<CreateSubstanceGUI>() == null)
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
            substanceSelected = TutorialSubstanceCreate.SubstanceSelected(voxelArray)
                || guiGameObject.GetComponent<EntityPickerGUI>() != null;
            return TutorialAction.NONE;
        }

        public override string GetText()
        {
            if (!substanceSelected)
                return "<i>Tap the substance to select it again.</i>";
            else
                return base.GetText();
        }

        public override bool ShowNextButton()
        {
            return substanceSelected;
        }
    }


    private class TutorialSubstanceAddBehavior : TutorialSubstancePage
    {
        public TutorialSubstanceAddBehavior()
            : base("<i>Try adding a Move behavior to the Substance.</i> Notice that behaviors are organized into multiple categories.",
            highlight: "add behavior") { }

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

        public override bool ShowNextButton()
        {
            return false;
        }
    }


    private class TutorialSubstanceFollowPlayer : TutorialSubstancePage
    {
        public TutorialSubstanceFollowPlayer()
            : base("When you play the game now, the substance will move West at a constant rate. "
            + "<i>Now try making the substance follow the player.</i>") { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is Substance)
                    foreach (EntityBehavior behavior in e.behaviors)
                        if (behavior is MoveBehavior)
                            foreach (Property prop in behavior.Properties())
                                if (prop.name == "Toward"
                                    && ((Target)(prop.value)).entityRef.entity is PlayerObject)
                                    return TutorialAction.NEXT;
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton()
        {
            return false;
        }
    }


    private class TutorialSubstanceAddSensor : TutorialSubstancePage
    {
        public TutorialSubstanceAddSensor()
            : base("Substances can have <i>Sensors</i> which turn On and Off in response to events in the game. "
            + "<i>Add a touch sensor to the substance.</i>",
            highlight: "change sensor") { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is Substance && e.sensor is TouchSensor)
                    return TutorialAction.NEXT;
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton()
        {
            return false;
        }
    }


    private class TutorialSubstanceTouchPlayer : TutorialSubstancePage
    {
        public TutorialSubstanceTouchPlayer()
            : base("<i>Now configure the touch sensor so it only turns on when touching the player.</i>") { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is Substance && e.sensor is TouchSensor)
                    foreach(Property prop in e.sensor.Properties())
                        if(prop.name == "Filter" && prop.value is ActivatedSensor.EntityFilter
                            && ((ActivatedSensor.EntityFilter)(prop.value)).entityRef.entity is PlayerObject)
                            return TutorialAction.NEXT;
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton()
        {
            return false;
        }
    }


    private class TutorialSubstanceHurt : TutorialSubstancePage
    {
        public TutorialSubstanceHurt()
            : base("<i>Add a Hurt behavior which hurts the substance by -100 points when it touches the player.</i>") { }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            base.Update(voxelArray, guiGameObject, touchListener);

            foreach (Entity e in voxelArray.GetSelectedEntities())
                if (e is Substance)
                    foreach (EntityBehavior behavior in e.behaviors)
                        if (behavior is HurtHealBehavior && behavior.condition == EntityBehavior.Condition.ON)
                            foreach (Property prop in behavior.Properties())
                                if (prop.name == "Amount" && (float)(prop.value) == -100.0f)
                                    return TutorialAction.NEXT;
            return TutorialAction.NONE;
        }

        public override bool ShowNextButton()
        {
            return false;
        }
    }
}