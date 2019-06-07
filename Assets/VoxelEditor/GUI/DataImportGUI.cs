using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataImportGUI : GUIPanel
{
    public EmbeddedDataType type;
    public System.Action<EmbeddedData> dataAction;

    private List<string> worldPaths = new List<string>();
    private List<string> worldNames = new List<string>();
    private bool worldSelected;
    private List<EmbeddedData> dataList;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            safeRect.width * .7f, safeRect.height * .9f);
    }

    void Start()
    {
        WorldFiles.ListWorlds(worldPaths, worldNames);
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
            }
            if (dataList != null)
            {
                foreach (EmbeddedData data in dataList)
                {
                    if (GUILayout.Button(data.name, GUIStyleSet.instance.buttonLarge))
                    {
                        dataAction(data);
                        Destroy(this);
                    }
                }
            }
        }
    }
}
