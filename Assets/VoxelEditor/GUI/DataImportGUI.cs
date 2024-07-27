using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface AudioPlayer {
    void Stop();
}

public delegate AudioPlayer AudioPlayerFactory(byte[] data);

public class DataImportGUI : GUIPanel {
    public EmbeddedDataType type;
    public System.Action<EmbeddedData> dataAction;
    public AudioPlayerFactory playerFactory;

    private List<string> worldPaths = new List<string>();
    private List<string> worldNames = new List<string>();
    private bool worldSelected, loadingWorld;
    private string selectedWorldName;
    private string errorMessage = null;
    private List<EmbeddedData> dataList;
    private AudioPlayer playingAudio;
    private EmbeddedData playingData;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            safeRect.width * .6f, safeRect.height * .9f, maxWidth: 1280, maxHeight: 1360);

    void Start() {
        WorldFiles.ListWorlds(worldPaths, worldNames);
        EditorFile.instance.importWorldHandler = ImportWorldHandler;
    }

    void OnDestroy() {
        StopPlayer();
        EditorFile.instance.importWorldHandler = null;
    }

    private EmbeddedData StopPlayer() {
        playingAudio?.Stop();
        playingAudio = null;
        var data = playingData;
        playingData = null;
        return data;
    }

    public override void WindowGUI() {
        if (loadingWorld) {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(StringSet.LoadingWorld);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        } else if (!worldSelected) {
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GUIUtils.PadContent(StringSet.ImportFile, IconSet.import),
                    StyleSet.buttonLarge)) {
                NativeGalleryWrapper.ImportAudioStream(ImportWorldHandler);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label(StringSet.ImportFromWorldHeader);
            for (int i = 0; i < worldPaths.Count; i++) {
                string path = worldPaths[i];
                string name = worldNames[i];

                if (GUILayout.Button(name, MenuGUI.worldButtonStyle.Value)) {
                    worldSelected = true;
                    selectedWorldName = name;
                    StartCoroutine(LoadWorldCoroutine(path));
                    scroll = Vector2.zero;
                    scrollVelocity = Vector2.zero;
                }
            }
            GUILayout.EndScrollView();
        } else { // world is selected
            GUILayout.BeginHorizontal();
            if (ActionBarGUI.ActionBarButton(IconSet.close)) {
                worldSelected = false;
                dataList = null;
                scroll = Vector2.zero;
                scrollVelocity = Vector2.zero;
                StopPlayer();
            }
            // prevent from expanding window
            GUIUtils.BeginHorizontalClipped(GUILayout.ExpandHeight(false));
            GUILayout.Label(selectedWorldName, MaterialSelectorGUI.categoryLabelStyle.Value);
            GUIUtils.EndHorizontalClipped();
            GUILayout.EndHorizontal();
            if (dataList != null && dataList.Count > 0) {
                scroll = GUILayout.BeginScrollView(scroll);
                foreach (EmbeddedData data in dataList) {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(
                            data.name, MenuGUI.worldButtonStyle.Value, GUILayout.MinWidth(0))) {
                        dataAction(data);
                        Destroy(this);
                    }
                    if (playerFactory != null && GUIUtils.HighlightedButton(IconSet.playAudio,
                            StyleSet.buttonLarge, playingData == data, GUILayout.ExpandWidth(false))) {
                        if (StopPlayer() != data) {
                            playingAudio = playerFactory(data.bytes);
                            playingData = data;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            } else {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (errorMessage != null) {
                    ActionBarGUI.ActionBarLabel(errorMessage);
                } else {
                    ActionBarGUI.ActionBarLabel(StringSet.NoDataInWorld(type.ToString())); // TODO: localize
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
    }


    // disposes stream when done
    private IEnumerator LoadWorldCoroutine(string path = null, System.IO.Stream stream = null) {
        loadingWorld = true;
        errorMessage = null;
        yield return null;
        yield return null;
        try {
            if (stream != null) {
                dataList = ReadWorldFile.ReadEmbeddedData(stream, type);
            } else {
                dataList = ReadWorldFile.ReadEmbeddedData(path, type);
            }
            foreach (EmbeddedData data in dataList) {
                Debug.Log(data.name);
            }
        } catch (MapReadException e) {
            errorMessage = e.Message;
            Debug.LogError(e.InnerException);
        } finally {
            loadingWorld = false;
            if (stream != null) {
                stream.Dispose();
                ShareMap.ClearFileWaitingToImport();
            }
        }
    }

    private void ImportWorldHandler(System.IO.Stream stream) {
        if (stream == null) {
            return;
        }
        StopPlayer();
        dataList = null;
        worldSelected = true;
        loadingWorld = true;
        StartCoroutine(LoadWorldCoroutine(stream: stream));
    }
}
