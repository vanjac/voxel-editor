using UnityEngine;

public class MenuOverflowGUI : GUIPanel {
    public TextAsset creditsText;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        new Rect(safeRect.xMin + safeRect.width * .8f, safeRect.yMin,
            safeRect.width * .2f, 0);

    public override GUIStyle GetStyle() => GUIStyle.none;

    void Start() {
        GUIPanel.topPanel = this;
    }

    public override void OnEnable() {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    public override void WindowGUI() {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (ActionBarGUI.ActionBarButton(IconSet.overflow)) {
            var overflow = gameObject.AddComponent<OverflowMenuGUI>();
            overflow.items = new OverflowMenuGUI.MenuItem[] {
                new OverflowMenuGUI.MenuItem(StringSet.OpenHelp, IconSet.help, () => {
                    gameObject.AddComponent<HelpGUI>();
                }),
                new OverflowMenuGUI.MenuItem(StringSet.OpenAbout, IconSet.about, () => {
#if false
                    string donate = StringSet.DonateMessage + "\n\n";
#else
                    string donate = "";
#endif
                    string assetCredits = AssetPack.Current().LoadConfigFile("info");
                    string debugInfo =
                        $"Build: {Application.buildGUID}\n"
                        + $"Resolution: {Screen.width}x{Screen.height}\nDPI: {Screen.dpi}\n"
                        + $"Audio: {AudioSettings.outputSampleRate}Hz {AudioSettings.speakerMode}";
                    string text = StringSet.AboutMessage(Application.version,
                        Application.unityVersion, donate, creditsText.text, assetCredits, debugInfo);
                    LargeMessageGUI.ShowLargeMessageDialog(gameObject, text);
                }),
                new OverflowMenuGUI.MenuItem(StringSet.OpenWebsite, IconSet.website, () => {
                    Application.OpenURL("https://chroma.zone/voxel-editor/");
                }),
                new OverflowMenuGUI.MenuItem(StringSet.OpenSubreddit, IconSet.reddit, () => {
                    Application.OpenURL("https://www.reddit.com/r/nspace/");
                }),
#if false
                new OverflowMenuGUI.MenuItem(StringSet.Donate, IconSet.donate, () => {
                    Application.OpenURL("https://chroma.zone/donate");
                }),
#endif
#if false
                new OverflowMenuGUI.MenuItem(StringSet.Language, IconSet.language, () => {
                    var languageMenu = gameObject.AddComponent<OverflowMenuGUI>();
                    languageMenu.stealFocus = true;
                    languageMenu.depth = 1;
                    languageMenu.items = new OverflowMenuGUI.MenuItem[] {
                        new OverflowMenuGUI.MenuItem(StringSet.LanguageAuto, null, () => {
                            GUIManager.SetLanguage(GUIManager.Language.Auto);
                        }),
                        new OverflowMenuGUI.MenuItem("English", null, () => {
                            GUIManager.SetLanguage(GUIManager.Language.English);
                        }),
                        new OverflowMenuGUI.MenuItem("Português", null, () => {
                            GUIManager.SetLanguage(GUIManager.Language.Portuguese);
                        }),
                    };
                }, stayOpen: true),
#endif
            };
        }
        GUILayout.EndHorizontal();
    }
}
