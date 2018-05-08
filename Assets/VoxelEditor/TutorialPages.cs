using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorials
{
    public enum PageId : int
    {
        NONE,
        END,
        INTRO_WELCOME,
        INTRO_ROOM,
        INTRO_NAVIGATION,
        INTRO_SELECT_FACE,
        INTRO_PULL,
        INTRO_PUSH,
        INTRO_COLUMN,
        INTRO_SELECT_BOX,
        INTRO_SELECT_WALL,
        INTRO_SUMMARY,
        INTRO_BUTTONS,
        INTRO_END,
        PAINT_START,
        PAINT_INTRO,
        PAINT_TEXTURES,
        PAINT_OVERLAYS,
        PAINT_TRANSFORM,
        PAINT_SKY,
        MAX_PAGES
    }

    public delegate TutorialPage TutorialPageFactory();
    public static TutorialPageFactory[] PAGES = new TutorialPageFactory[(int)PageId.MAX_PAGES];

    static Tutorials()
    {
        PAGES[(int)PageId.NONE] = () => null;
        PAGES[(int)PageId.INTRO_WELCOME] = () => new SimpleTutorialPage(
            "Welcome! This is a brief tutorial that will guide you through the app. "
            + "You can access this tutorial and others at any time. Press the right arrow to continue.",
            next: PageId.INTRO_ROOM);
        PAGES[(int)PageId.INTRO_ROOM] = () => new SimpleTutorialPage(
            "Right now you are looking at the interior of a room. One wall is hidden so you can see inside. "
            + "The green person marks the player location.",
            next: PageId.INTRO_NAVIGATION);
        PAGES[(int)PageId.INTRO_NAVIGATION] = () => new TutorialIntroNavigation();
        PAGES[(int)PageId.INTRO_SELECT_FACE] = () => new TutorialIntroSelectFace();
        PAGES[(int)PageId.INTRO_PULL] = () => new TutorialIntroPull();
        PAGES[(int)PageId.INTRO_PUSH] = () => new TutorialIntroPush();
        PAGES[(int)PageId.INTRO_COLUMN] = () => new TutorialIntroColumn();
        PAGES[(int)PageId.INTRO_SELECT_BOX] = () => new TutorialIntroSelectBox();
        PAGES[(int)PageId.INTRO_SELECT_WALL] = () => new TutorialIntroSelectWall();
        PAGES[(int)PageId.INTRO_SUMMARY] = () => new SimpleTutorialPage(
            "By selecting faces and pushing/pulling them, you can sculpt the world.",
            next: PageId.INTRO_BUTTONS);
        PAGES[(int)PageId.INTRO_BUTTONS] = () => new FullScreenTutorialPage(
            "These buttons appear at the top of the screen, based on context.",
            "Tutorials/toolbar_buttons", scale: 1.5f, next: PageId.INTRO_END);
        PAGES[(int)PageId.INTRO_END] = () => new SimpleTutorialPage(
            "Good luck! You can access more tutorials by choosing Help in the menu.",
            next: PageId.END);

        PAGES[(int)PageId.PAINT_START] = () => new TutorialPaintStart();
        PAGES[(int)PageId.PAINT_INTRO] = () => new TutorialPaintPage(
            "You can use the Paint panel to paint the selected faces with <i>materials</i> and <i>overlays</i>.",
            next: PageId.PAINT_TEXTURES);
        PAGES[(int)PageId.PAINT_TEXTURES] = () => new TutorialPaintPage(
            "Choose any of the categories in the list to browse textures. Or switch to the Color tab to paint a solid color.",
            next: PageId.PAINT_OVERLAYS);
        PAGES[(int)PageId.PAINT_OVERLAYS] = () => new TutorialPaintPage(
            "A paint is composed of two parts: an opaque material and a transparent overlay. "
            + "Use the tabs to switch between the two parts.",
            next: PageId.PAINT_TRANSFORM);
        PAGES[(int)PageId.PAINT_TRANSFORM] = () => new TutorialPaintPage(
            "Use these buttons to rotate and mirror the paint.",
            next: PageId.PAINT_SKY);
        PAGES[(int)PageId.PAINT_SKY] = () => new TutorialPaintSky();
    }

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

        public override PageId Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
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
                return PageId.INTRO_SELECT_FACE;
            else
                return PageId.NONE;
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
            voxelArray.ClearSelection();
            voxelArray.ClearStoredSelection();
        }

        public override PageId Update(VoxelArrayEditor voxelArray, GameObject guiGameObject,
            TouchListener touchListener)
        {
            if (voxelArray.FacesAreSelected())
                return PageId.INTRO_PULL;
            else
                return PageId.NONE;
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

        public override PageId Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (!voxelArray.FacesAreSelected())
                return PageId.INTRO_SELECT_FACE;
            faceNormal = voxelArray.GetSelectedFaceNormal();
            if (touchListener.currentTouchOperation == TouchListener.TouchOperation.MOVE
                && AxisMatchesFace(touchListener.movingAxis, faceNormal))
            {
                int moveCount = touchListener.movingAxis.moveCount;
                if (faceNormal % 2 == 0)
                {
                    if (moveCount <= -1)
                        return PageId.INTRO_PUSH;
                }
                else
                {
                    if (moveCount >= 1)
                        return PageId.INTRO_PUSH;
                }
            }
            return PageId.NONE;
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

        public override PageId Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
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
                        return PageId.INTRO_COLUMN;
                }
                else
                {
                    if (moveCount <= -1)
                        return PageId.INTRO_COLUMN;
                }
            }
            return PageId.NONE;
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

        public override PageId Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            int faceNormal = voxelArray.GetSelectedFaceNormal();
            if (faceNormal != -1)
                lastFaceNormal = faceNormal;
            if (touchListener.currentTouchOperation == TouchListener.TouchOperation.MOVE
                && TutorialIntroPull.AxisMatchesFace(touchListener.movingAxis, lastFaceNormal)
                && !voxelArray.SomethingIsSelected())
                // just made a column
                return PageId.INTRO_SELECT_BOX;
            else
                return PageId.NONE;
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

        public override PageId Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
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
                return PageId.INTRO_SELECT_WALL;
            else
                return PageId.NONE;
        }
    }


    private class TutorialIntroSelectWall : TutorialPage
    {
        public override string GetText()
        {
            return "<i>Double tap to select an entire wall.</i>";
        }

        public override PageId Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (voxelArray.selectMode == VoxelArrayEditor.SelectMode.FACE)
                return PageId.INTRO_SUMMARY;
            else
                return PageId.NONE;
        }
    }


    private class TutorialPaintStart : TutorialPage
    {
        public override string GetText()
        {
            return "<i>Select a face and tap the paint roller icon to open the Paint panel.</i>";
        }

        public override PageId Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            if (guiGameObject.GetComponent<PaintGUI>() != null)
                return PageId.PAINT_INTRO;
            else
                return PageId.NONE;
        }
    }


    private class TutorialPaintPage : TutorialPage
    {
        private readonly string text;
        private Tutorials.PageId next;
        private bool panelOpen;

        public TutorialPaintPage(string text, Tutorials.PageId next)
        {
            this.text = text;
            this.next = next;
        }

        public override PageId Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            panelOpen = guiGameObject.GetComponent<PaintGUI>() != null;
            return PageId.NONE;
        }

        public override string GetText()
        {
            if (!panelOpen)
                return "<i>Reopen the paint panel. (Select a face and tap the paint roller icon)</i>";
            else
                return text;
        }

        public override PageId GetNextButtonTarget()
        {
            if (!panelOpen)
                return PageId.NONE;
            else
                return next;
        }
    }


    private class TutorialPaintSky : TutorialPaintPage
    {
        public TutorialPaintSky()
            : base("The “Sky” material is special: in the game it is an unobstructed window to the sky. "
                + "Since the world can't have holes, this is the only way to see the sky.",
                next: PageId.END) { }

        public override void Start(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
            PaintGUI paintPanel = guiGameObject.GetComponent<PaintGUI>();
            paintPanel.TutorialShowSky();
        }
    }
}