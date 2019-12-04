using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface AudioPlayer
{
    void Stop();
}

public delegate AudioPlayer AudioPlayerFactory(byte[] data);

public class DataImportGUI : GUIPanel
{
    public EmbeddedDataType type;
    public System.Action<EmbeddedData> dataAction;
    public AudioPlayerFactory playerFactory;

    private List<string> worldPaths = new List<string>();
    private List<string> worldNames = new List<string>();
    private bool worldSelected, loadingWorld;
    private List<EmbeddedData> dataList;
    private AudioPlayer playingAudio;
    private EmbeddedData playingData;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            safeRect.width * .6f, safeRect.height * .9f);
    }

    void Start()
    {
        WorldFiles.ListWorlds(worldPaths, worldNames);
        EditorFile.instance.importWorldHandler = ImportWorldHandler;
    }

    void OnDestroy()
    {
        StopPlayer();
        EditorFile.instance.importWorldHandler = null;
    }

    private EmbeddedData StopPlayer()
    {
        if (playingAudio != null)
            playingAudio.Stop();
        playingAudio = null;
        var data = playingData;
        playingData = null;
        return data;
    }

    public override void WindowGUI()
    {
        if (loadingWorld)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Loading world...");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        else if (!worldSelected)
        {
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Choose a file to open in N-Space", GUIStyleSet.instance.buttonLarge))
                ShareMap.OpenFileManager();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("Or import from a world...");
            for (int i = 0; i < worldPaths.Count; i++)
            {
                string path = worldPaths[i];
                string name = worldNames[i];

                if (GUILayout.Button(name, GUIStyleSet.instance.buttonLarge))
                {
                    worldSelected = true;
                    StartCoroutine(LoadWorldCoroutine(path));
                    scroll = Vector2.zero;
                    scrollVelocity = Vector2.zero;
                }
            }
            GUILayout.EndScrollView();
        }
        else // world is selected
        {
            if (GUIUtils.HighlightedButton("Back to world list", GUIStyleSet.instance.buttonLarge))
            {
                worldSelected = false;
                dataList = null;
                scroll = Vector2.zero;
                scrollVelocity = Vector2.zero;
                StopPlayer();
            }
            if (dataList != null && dataList.Count > 0)
            {
                foreach (EmbeddedData data in dataList)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(data.name, GUIStyleSet.instance.buttonLarge))
                    {
                        dataAction(data);
                        Destroy(this);
                    }
                    if (playerFactory != null && GUIUtils.HighlightedButton(
                        GUIIconSet.instance.playAudio,
                        GUIStyleSet.instance.buttonLarge,
                        playingData == data,
                        GUILayout.ExpandWidth(false)))
                    {
                        if (StopPlayer() != data)
                        {
                            playingAudio = playerFactory(data.bytes);
                            playingData = data;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                ActionBarGUI.ActionBarLabel("World contains no " + type.ToString() + " files.");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
    }


    private IEnumerator LoadWorldCoroutine(string path = null, System.IO.Stream stream = null)
    {
        loadingWorld = true;
        yield return null;
        yield return null;
        try
        {
            if (stream != null)
                dataList = ReadWorldFile.ReadEmbeddedData(stream, type);
            else
                dataList = ReadWorldFile.ReadEmbeddedData(path, type);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            loadingWorld = false;
            stream.Close();
            ShareMap.ClearFileWaitingToImport();
        }
    }

    private void ImportWorldHandler(System.IO.Stream stream)
    {
        StopPlayer();
        dataList = null;
        worldSelected = true;
        loadingWorld = true;
        StartCoroutine(LoadWorldCoroutine(stream: stream));
    }
}
