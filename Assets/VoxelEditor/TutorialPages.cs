public class Tutorials
{
    public enum PageId : int
    {
        NONE,
        INTRO_WELCOME,
        TEST_PAGE,
        MAX_PAGES
    }

    public static TutorialPage[] PAGES = new TutorialPage[(int)PageId.MAX_PAGES];

    static Tutorials()
    {
        PAGES[(int)PageId.INTRO_WELCOME] = new TutorialPage("Welcome to the tutorials!",
            next:PageId.TEST_PAGE);
        PAGES[(int)PageId.TEST_PAGE] = new TutorialPage("Test Page");
    }
}