using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBarGUI : GUIPanel
{
    public VoxelArray voxelArray;
    public Transform cameraPivot;

    public override void OnGUI()
    {
        base.OnGUI();

        panelRect = new Rect(190, 10, scaledScreenWidth - 190, 20);

        if (GUI.Button(new Rect(panelRect.xMin, panelRect.yMin, 80, 20), "Save"))
        {
            MapFileWriter writer = new MapFileWriter("mapsave");
            writer.Write(cameraPivot, voxelArray);
        }

        if (GUI.Button(new Rect(panelRect.xMin + 90, panelRect.yMin, 80, 20), "Load"))
        {
            MapFileReader reader = new MapFileReader("mapsave");
            reader.Read(cameraPivot, voxelArray);
        }
    }
}
