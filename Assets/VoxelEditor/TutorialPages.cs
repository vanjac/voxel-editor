using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorials
{
    public enum PageId : int
    {
        NONE,
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
        MAX_PAGES
    }

    public delegate TutorialPage TutorialPageFactory();
    public static TutorialPageFactory[] PAGES = new TutorialPageFactory[(int)PageId.MAX_PAGES];

    static Tutorials()
    {
        PAGES[(int)PageId.NONE] = () => null;
        PAGES[(int)PageId.INTRO_WELCOME] = () => new SimpleTutorialPage(
            "Welcome to _________! This is a brief tutorial that will guide you through the app. "
            + "You can access this tutorial and others at any time. Press Right to continue.",
            next: PageId.INTRO_ROOM);
        PAGES[(int)PageId.INTRO_ROOM] = () => new SimpleTutorialPage(
            "Right now you are looking at the interior of a room. One wall is hidden so you can see inside. "
            + "The green person marks the player start location.",
            next: PageId.INTRO_NAVIGATION);
        PAGES[(int)PageId.INTRO_NAVIGATION] = () => new TutorialIntroNavigation();
        PAGES[(int)PageId.INTRO_SELECT_FACE] = () => new TutorialIntroSelectFace();
        PAGES[(int)PageId.INTRO_PULL] = () => new TutorialIntroPull();
        PAGES[(int)PageId.INTRO_PUSH] = () => new TutorialIntroPush();
        PAGES[(int)PageId.INTRO_COLUMN] = () => new TutorialIntroColumn();
        PAGES[(int)PageId.INTRO_SELECT_BOX] = () => new SimpleTutorialPage(
            "Tap and drag to select a group of faces in a rectangle or box. <i>Try this a few times.</i>",
            next: PageId.INTRO_SELECT_WALL);
        PAGES[(int)PageId.INTRO_SELECT_WALL] = () => new SimpleTutorialPage(
            "<i>Double tap to select an entire wall.</i>",
            next: PageId.INTRO_SUMMARY);
        PAGES[(int)PageId.INTRO_SUMMARY] = () => new SimpleTutorialPage(
            "By selecting faces and pushing/pulling them, you can sculpt the world.",
            next: PageId.INTRO_BUTTONS);
        PAGES[(int)PageId.INTRO_BUTTONS] = () => new SimpleTutorialPage(
            "[toolbar buttons]",
            next: PageId.INTRO_END);
        PAGES[(int)PageId.INTRO_END] = () => new SimpleTutorialPage(
            "Good luck!");
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
        private int faceNormal;

        public override string GetText()
        {
            string message = "<i>Now select a different face and push it away from the center of the room.</i>";
            if (faceNormal != -1)
                message += " <i>(hint: use the " + TutorialIntroPull.AxisNameForFaceNormal(faceNormal)
                    + " arrow)</i>";
            return message;
        }

        public override PageId Update(VoxelArrayEditor voxelArray, GameObject guiGameObject, TouchListener touchListener)
        {
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
        int lastFaceNormal = -1;

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
}