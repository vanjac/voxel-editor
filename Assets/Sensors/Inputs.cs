using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputsSensor : Sensor
{
    // public so it can be serialized
    // this is serialized so don't change it!
    public struct Input
    {
        public EntityReference entityRef;
        public sbyte onChange;
        public sbyte offChange;

        public Input(Entity entity)
        {
            entityRef = new EntityReference(entity);
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

        int inputToDelete = -1;
        for (int i = 0; i < inputs.Length; i++)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(inputs[i].entityRef.entity.TypeName() + " ");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X"))
                inputToDelete = i;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUIStyle changeGridStyle = new GUIStyle(GUI.skin.button);
            changeGridStyle.padding = new RectOffset(0, 0, 16, 16);
            changeGridStyle.margin = new RectOffset(0, 0, 0, 0);
            GUIStyle newLabelStyle = new GUIStyle(GUI.skin.label);
            newLabelStyle.padding = new RectOffset();
            GUILayout.Label("On: ", newLabelStyle, GUILayout.ExpandWidth(false));
            inputs[i].onChange = (sbyte)(GUILayout.SelectionGrid(inputs[i].onChange + 1,
                new string[] { "-1", "0", "1" }, 3, changeGridStyle) - 1);
            GUILayout.Label("Off: ", newLabelStyle, GUILayout.ExpandWidth(false));
            inputs[i].offChange = (sbyte)(GUILayout.SelectionGrid(inputs[i].offChange + 1,
                new string[] { "-1", "0", "1" }, 3, changeGridStyle) - 1);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        if (inputToDelete != -1)
        {
            Input[] newInputs = new Input[inputs.Length - 1];
            Array.Copy(inputs, newInputs, inputToDelete);
            Array.Copy(inputs, inputToDelete + 1, newInputs, inputToDelete, newInputs.Length - inputToDelete);
            inputs = newInputs;
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