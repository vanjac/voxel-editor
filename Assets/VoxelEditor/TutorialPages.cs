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

    private class TutorialIntroNavigation : TutorialPage
    {
        private Quaternion startRotation;
        private float startZoom;
        private Vector3 startPan;
        private bool rotate, zoom, pan;
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
            startZoom = touchListener.pivot.localScale.x;
            startPan = touchListener.pivot.position;
            startTime = Time.time;
        }

        public override TutorialAction Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            if (!rotate)
            {
                var currentRotation = touchListener.pivot.rotation;
                if (Quaternion.Angle(currentRotation, startRotation) > 90)
                {
                    Debug.Log("Rotate complete");
                    rotate = true;
                }
            }
            if (!zoom)
            {
                var zoomAmount = touchListener.pivot.localScale.x / startZoom;
                if (zoomAmount > 2 || zoomAmount < 0.5f)
                {
                    Debug.Log("Zoom complete");
                    zoom = true;
                }
            }
            if (!pan)
            {
                var currentPan = touchListener.pivot.position;
                if ((currentPan - startPan).magnitude > 5)
                {
                    Debug.Log("Pan complete");
                    pan = true;
                }
            }
            if (rotate && zoom && pan && Time.time - startTime > 5)
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

        public static bool AxisMatchesFace(MoveAxis moveAxis, int faceNormal)
        {
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
                int moveCount = touchListener.movingAxis.moveCount;
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
                int moveCount = touchListener.movingAxis.moveCount;
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

        public override bool ShowNextButton()
        {
            return panelOpen;
        }
    }


    private class TutorialPaintSky : TutorialPaintPage
    {
        public TutorialPaintSky()
            : base("The “Sky” material is special: in the game it is an unobstructed window to the sky. "
                + "Since the world can't have holes, this is the only way to see the sky.") { }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            PaintGUI paintPanel = guiGameObject.GetComponent<PaintGUI>();
            paintPanel.TutorialShowSky();
        }
    }
}