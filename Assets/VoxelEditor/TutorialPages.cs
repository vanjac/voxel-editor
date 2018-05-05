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
            new SimpleTutorialPage("Welcome to the tutorials!", next:PageId.TEST_PAGE);
        PAGES[(int)PageId.TEST_PAGE] = () =>
            new SimpleTutorialPage("Test Page");
    }
}