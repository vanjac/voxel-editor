using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputsSensor : Sensor
{
    // public so it can be serialized
    public struct Input
    {
        Entity entity;
        byte onChange;
        byte offChange;
    }

    public int threshold;
    private Input[] inputs = new Input[0];

    public override string TypeName()
    {
        return "Inputs";
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("Threshold",
                () => threshold,
                v => threshold = (int)v,
                PropertyGUIs.Int),
            new Property("Inputs",
                () => inputs,
                v => inputs = (Input[])v,
                InputsGUI)
        }, base.Properties());
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        throw new System.NotImplementedException();
    }

    private object InputsGUI(object value)
    {
        if (GUILayout.Button("Add Input"))
        {
            // TODO: do all this without using GameObject.Find
            EntityPickerGUI picker = GameObject.Find("GUI").AddComponent<EntityPickerGUI>();
            picker.voxelArray = GameObject.Find("VoxelArray").GetComponent<VoxelArrayEditor>();
            picker.handler = (ICollection<Entity> entities) =>
            {
                Debug.Log(entities);
            };
        }
        return value;
    }
}