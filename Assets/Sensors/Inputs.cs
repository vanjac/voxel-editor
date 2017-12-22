using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputsSensor : Sensor
{
    // public so it can be serialized
    public struct Input
    {
        public Entity entity;
        public sbyte onChange;
        public sbyte offChange;

        public Input(Entity entity)
        {
            this.entity = entity;
            onChange = 1;
            offChange = -1;
        }
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

    private static Input[] handlerResult = null;

    private object InputsGUI(object value)
    {
        Input[] inputs;
        if (handlerResult != null)
        {
            inputs = handlerResult;
            handlerResult = null;
        }
        else
            inputs = (Input[])value;

        foreach (Input input in inputs)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(input.entity.TypeName());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Input"))
        {
            // TODO: do all this without using GameObject.Find
            EntityPickerGUI picker = GameObject.Find("GUI").AddComponent<EntityPickerGUI>();
            picker.voxelArray = GameObject.Find("VoxelArray").GetComponent<VoxelArrayEditor>();
            picker.handler = (ICollection<Entity> entities) =>
            {
                handlerResult = new Input[inputs.Length + entities.Count];
                Array.Copy(inputs, handlerResult, inputs.Length);
                int i = 0;
                foreach (Entity entity in entities)
                {
                    handlerResult[inputs.Length + i] = new Input(entity);
                    i++;
                }
            };
        }

        return inputs;
    }
}