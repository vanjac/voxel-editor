public class Tutorials
{
    public enum PageId : int
    {
        NONE,
        INTRO_WELCOME,
        TEST_PAGE,
        MAX_PAGES
    }

    public delegate TutorialPage TutorialPageFactory();
    public static TutorialPageFactory[] PAGES = new TutorialPageFactory[(int)PageId.MAX_PAGES];

    static Tutorials()
    {
        PAGES[(int)PageId.NONE] = () => null;
        PAGES[(int)PageId.INTRO_WELCOME] = () =>
            new SimpleTutorialPage("Welcome to _________! This is a brief tutorial that will guide you through the app. "
                                   + "You can access this tutorial and others at any time. Press Right to continue.",
                                   next:PageId.TEST_PAGE);
        PAGES[(int)PageId.TEST_PAGE] = () =>
            new SimpleTutorialPage("Test Page");
    }
}