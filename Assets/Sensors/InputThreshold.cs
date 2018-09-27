using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputThresholdSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Threshold", "Active when a certain threshold of other objects are active",
        "The values of all of the inputs are continuously added up. "
        + "If an Input is on and set to \"Positive\", the total is incremented by one. "
        + "If an Input is on and set to \"Negative\", the total is decremented by one. "
        + "The sensor turns on if the total is greater than or equal to the threshold.\n\n"
        + "Activators: the combined activators of all positive inputs minus the activators of negative inputs",
        "altimeter", typeof(InputThresholdSensor));

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
            Entity e = inputs[i].entityRef.entity;
            if (e == null)
                inputToDelete = i;
            EntityReferencePropertyManager.Next(e);
            GUI.color = baseColor * EntityReferencePropertyManager.GetColor();
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(EntityReferencePropertyManager.GetName() + " ");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X"))
                inputToDelete = i;
            GUILayout.EndHorizontal();

            int negativeNum = inputs[i].negative ? 1 : 0;
            int newNegativeNum = GUILayout.SelectionGrid(negativeNum,
                new string[] { "Positive", "Negative" }, 2, GUI.skin.GetStyle("button_tab"));
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
    private Dictionary<EntityComponent, int> activatorCounts = new Dictionary<EntityComponent, int>();
    private bool wasOn = false;

    void Update()
    {
        int energy = CalculateEnergy();
        bool isOn = energy >= threshold;

        if (isOn && !wasOn)
        {
            AddActivator(null); // make sure sensor is on
            foreach (EntityComponent activator in activatorCounts.Keys)
                AddActivator(activator);
        }
        else if (wasOn && !isOn)
        {
            ClearActivators();
        }
        wasOn = isOn;

        foreach (var input in inputs)
        {
            EntityComponent e = input.entityRef.component;
            if (e == null)
                continue;
            foreach (var newActivator in e.GetNewActivators())
            {
                if (input.negative)
                    DecrInputActivator(newActivator);
                else
                    IncrInputActivator(newActivator);
            }
            foreach (var removedActivator in e.GetRemovedActivators())
            {
                if (input.negative)
                    IncrInputActivator(removedActivator);
                else
                    DecrInputActivator(removedActivator);
            }
        }
    }

    private int GetInputActivatorCount(EntityComponent activator)
    {
        if (activatorCounts.ContainsKey(activator))
            return activatorCounts[activator];
        else
            return 0;
    }

    private void IncrInputActivator(EntityComponent activator)
    {
        if (activator == null)
            return; // always has null activator
        int newCount = GetInputActivatorCount(activator) + 1;
        //Debug.Log("Incr " + activator + " -> " + newCount);
        if (newCount == 0)
            activatorCounts.Remove(activator);
        else
        {
            activatorCounts[activator] = newCount;
            if (newCount == 1 && wasOn)
                AddActivator(activator);
        }
    }

    private void DecrInputActivator(EntityComponent activator)
    {
        if (activator == null)
            return; // always has null activator
        int newCount = GetInputActivatorCount(activator) - 1;
        //Debug.Log("Decr " + activator + " -> " + newCount);
        if (newCount == 0)
        {
            activatorCounts.Remove(activator);
            if (wasOn)
                RemoveActivator(activator);
        }
        else
            activatorCounts[activator] = newCount;
    }

    private int CalculateEnergy()
    {
        int energy = 0;
        foreach (var input in inputs)
        {
            EntityComponent e = input.entityRef.component;
            if (e != null && e.IsOn())
            {
                if (input.negative)
                    energy--;
                else
                    energy++;
            }
        }
        return energy;
    }
}