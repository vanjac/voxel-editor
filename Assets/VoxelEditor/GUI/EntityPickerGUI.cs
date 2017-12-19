using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPickerGUI : GUIPanel
{
    public delegate void EntityPickerHanlder(ICollection<Entity> entities);

    public VoxelArrayEditor voxelArray;
    public EntityPickerHanlder handler;

    public override void OnEnable()
    {
        holdOpen = true;
        base.OnEnable();
    }

    public void Start()
    {
        voxelArray.StoreSelection();
    }

    public override Rect GetRect(float width, float height)
    {
        return new Rect(height * .55f, height * .9f, height * .65f, height * .1f);
    }

    public override void WindowGUI()
    {
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
    }
}
