using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputThresholdSensor : Sensor
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

    public int threshold = 1;
    private Input[] inputs = new Input[0];

    public override string TypeName()
    {
        return "Input Threshold";
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
        InputThresholdComponent component = gameObject.AddComponent<InputThresholdComponent>();
        component.inputs = inputs;
        component.threshold = threshold;
        return component;
    }

    private void InputsGUI(Property property)
    {
        Input[] inputs = (Input[])property.value;

        GUILayout.Label("Inputs:");
        if (GUILayout.Button("Add Input"))
        {
            // TODO: do all this without using GameObject.Find
            EntityPickerGUI picker = GameObject.Find("GUI").AddComponent<EntityPickerGUI>();
            picker.voxelArray = GameObject.Find("VoxelArray").GetComponent<VoxelArrayEditor>();
            picker.handler = (ICollection<Entity> entities) =>
            {
                Input[] newInputs = new Input[inputs.Length + entities.Count];
                Array.Copy(inputs, newInputs, inputs.Length);
                int i = 0;
                foreach (Entity entity in entities)
                {
                    newInputs[inputs.Length + i] = new Input(entity);
                    i++;
                }
                property.value = newInputs;
            };
        }

        bool copyArray = false;
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
            sbyte oldOnChange = inputs[i].onChange;
            inputs[i].onChange = (sbyte)(GUILayout.SelectionGrid(oldOnChange + 1,
                new string[] { "-1", "0", "+1" }, 3, changeGridStyle) - 1);
            if (oldOnChange != inputs[i].onChange)
                copyArray = true;
            GUILayout.Label("Off: ", newLabelStyle, GUILayout.ExpandWidth(false));
            sbyte oldOffChange = inputs[i].offChange;
            inputs[i].offChange = (sbyte)(GUILayout.SelectionGrid(oldOffChange + 1,
                new string[] { "-1", "0", "+1" }, 3, changeGridStyle) - 1);
            if (oldOffChange != inputs[i].offChange)
                copyArray = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        if (inputToDelete != -1)
        {
            Input[] newInputs = new Input[inputs.Length - 1];
            Array.Copy(inputs, newInputs, inputToDelete);
            Array.Copy(inputs, inputToDelete + 1, newInputs, inputToDelete, newInputs.Length - inputToDelete);
            property.value = newInputs;
        }
        else if (copyArray)
        {
            Input[] newInputs = new Input[inputs.Length];
            Array.Copy(inputs, newInputs, inputs.Length);
            property.value = newInputs; // mark unsaved changes flag
        }
    }
}

public class InputThresholdComponent : SensorComponent
{
    public InputThresholdSensor.Input[] inputs;
    public float threshold;

    private float value = 0;
    private bool[] entitiesAreOn;

    void Start()
    {
        entitiesAreOn = new bool[inputs.Length];
        for (int i = 0; i < entitiesAreOn.Length; i++)
            entitiesAreOn[i] = false;
    }

    void Update()
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            bool isOn = inputs[i].entityRef.entity.component.IsOn();
            bool wasOn = entitiesAreOn[i];
            if (isOn && !wasOn)
            {
                entitiesAreOn[i] = true;
                value += inputs[i].onChange;
            }
            if (!isOn && wasOn)
            {
                entitiesAreOn[i] = false;
                value += inputs[i].offChange;
            }
        }
    }

    public override bool IsOn()
    {
        return value >= threshold;
    }
}