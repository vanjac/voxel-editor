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
        PAGES[(int)PageId.INTRO_NAVIGATION] = () => new SimpleTutorialPage(
            "Navigation: Use two fingers to rotate and zoom, and three fingers to pan. "
            + "<i>Try looking around the room.</i>",
            next: PageId.INTRO_SELECT_FACE);
        PAGES[(int)PageId.INTRO_SELECT_FACE] = () => new SimpleTutorialPage(
            "<i>Tap with one finger to select a single face of a block.</i>",
            next: PageId.INTRO_PULL);
        PAGES[(int)PageId.INTRO_PULL] = () => new SimpleTutorialPage(
            "<i>Pull the ____ arrow towards the center of the room to pull the block out.</i>",
            next: PageId.INTRO_PUSH);
        PAGES[(int)PageId.INTRO_PUSH] = () => new SimpleTutorialPage(
            "<i>Now select a different face and push it away from the center of the room.</i>",
            next: PageId.INTRO_COLUMN);
        PAGES[(int)PageId.INTRO_COLUMN] = () => new SimpleTutorialPage(
            "<i>Now select a different face and pull it towards the center of the room. "
            + "Keep pulling as far as you can.</i>",
            next: PageId.INTRO_SELECT_BOX);
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
}