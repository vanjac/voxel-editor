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
    private bool worldSelected;
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
    }

    void OnDestroy()
    {
        if (playingAudio != null)
            playingAudio.Stop();
    }

    public override void WindowGUI()
    {
        if (!worldSelected)
        {
            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < worldPaths.Count; i++)
            {
                string path = worldPaths[i];
                string name = worldNames[i];

                if (GUILayout.Button(name, GUIStyleSet.instance.buttonLarge))
                {
                    worldSelected = true;
                    try
                    {
                        dataList = ReadWorldFile.ReadEmbeddedData(path, type);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }
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
                if (playingAudio != null)
                {
                    playingAudio.Stop();
                    playingAudio = null;
                    playingData = null;
                }
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
                        if (playingAudio != null)
                            playingAudio.Stop();
                        if (playingData == data)
                        {
                            playingAudio = null;
                            playingData = null;
                        }
                        else
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
}
