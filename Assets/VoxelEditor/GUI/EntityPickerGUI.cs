using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPickerGUI : GUIPanel
{
    public delegate void EntityPickerHanlder(ICollection<Entity> entities);

    public VoxelArray voxelArray;
    public EntityPickerHanlder handler;

    public override void OnEnable()
    {
        depth = -1;
        holdOpen = true;
        base.OnEnable();
    }

    public void Start()
    {
        voxelArray.StoreSelection();
    }

    public override void OnGUI()
    {
        base.OnGUI();

        panelRect = new Rect(targetHeight * .55f, targetHeight * .9f, targetHeight * .65f, targetHeight * .1f);
        GUILayout.BeginArea(panelRect, GUI.skin.box);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Pick an object...");
        if (GUILayout.Button("Done"))
        {
            handler(voxelArray.GetSelectedEntities());
            voxelArray.ClearSelection();
            voxelArray.MergeStoredSelected();
            Destroy(this);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
