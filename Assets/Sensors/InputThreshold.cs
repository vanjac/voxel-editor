using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputThresholdSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Threshold", "Active when a certain threshold of other objects are active", "altimeter", typeof(InputThresholdSensor));

    // public so it can be serialized
    // this is serialized so don't change it!
    public struct Input
    {
        public EntityReference entityRef;
        public bool negative;

        public Input(Entity entity)
        {
            entityRef = new EntityReference(entity);
            negative = false;
        }
    }

    public int threshold = 1;
    private Input[] inputs = new Input[0];

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
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
            EntityPickerGUI picker = GUIPanel.guiGameObject.AddComponent<EntityPickerGUI>();
            picker.voxelArray = VoxelArrayEditor.instance;
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
        Color baseColor = GUI.color;
        for (int i = 0; i < inputs.Length; i++)
        {
            EntityReferencePropertyManager.Next(inputs[i].entityRef.entity);
            GUI.color = baseColor * EntityReferencePropertyManager.GetColor();
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(EntityReferencePropertyManager.GetName() + " ");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X"))
                inputToDelete = i;
            GUILayout.EndHorizontal();

            GUIStyle changeGridStyle = new GUIStyle(GUI.skin.button);
            changeGridStyle.padding = new RectOffset(0, 0, 16, 16);
            changeGridStyle.margin = new RectOffset(0, 0, 0, 0);

            int negativeNum = inputs[i].negative ? 1 : 0;
            int newNegativeNum = GUILayout.SelectionGrid(negativeNum,
                new string[] { "Positive", "Negative" }, 2, changeGridStyle);
            if (negativeNum != newNegativeNum)
            {
                inputs[i].negative = newNegativeNum == 1;
                copyArray = true;
            }
            GUILayout.EndVertical();
        }
        GUI.color = baseColor;
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

    private bool value = false;

    void Update()
    {
        int energy = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            EntityComponent e = inputs[i].entityRef.component;
            if (e != null && e.IsOn())
            {
                if (inputs[i].negative)
                    energy--;
                else
                    energy++;
            }
        }
        value = energy >= threshold;
    }

    public override bool IsOn()
    {
        return value;
    }
}