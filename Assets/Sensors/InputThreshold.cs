using System;
using System.Collections.Generic;
using UnityEngine;

public class InputThresholdSensor : GenericSensor<InputThresholdSensor, InputThresholdComponent>
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Threshold", s => s.ThresholdDesc, s => s.ThresholdLongDesc, "altimeter",
        typeof(InputThresholdSensor));
    public override PropertiesObjectType ObjectType => objectType;

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
    public Input[] inputs = new Input[0];

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("thr", s => s.PropThreshold,
                () => threshold,
                v => threshold = (int)v,
                PropertyGUIs.Int),
            new Property("inp", s => s.PropInputs,
                () => inputs,
                v => inputs = (Input[])v,
                InputsGUI)
        }, base.Properties());

    private void InputsGUI(Property property)
    {
        Input[] inputs = (Input[])property.value;

        GUILayout.Label(GUIPanel.StringSet.InputsHeader);
        if (GUILayout.Button(
            GUIUtils.PadContent(GUIPanel.StringSet.AddInput, GUIPanel.IconSet.newItem)))
        {
            EntityPickerGUI picker = GUIPanel.GuiGameObject.AddComponent<EntityPickerGUI>();
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
            GUI.color = baseColor;
            GUILayout.Label(EntityReferencePropertyManager.GetName() + " ");

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            int negativeNum = inputs[i].negative ? 1 : 0;
            int newNegativeNum = GUILayout.SelectionGrid(negativeNum,
                new Texture[] { GUIPanel.IconSet.plusOne, GUIPanel.IconSet.minusOne }, 2,
                GUIPanel.StyleSet.buttonSmall, GUILayout.ExpandWidth(false));
            if (negativeNum != newNegativeNum)
            {
                inputs[i].negative = newNegativeNum == 1;
                copyArray = true;
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GUIPanel.IconSet.delete, GUIPanel.StyleSet.buttonSmall,
                    GUILayout.ExpandWidth(false)))
            {
                inputToDelete = i;
            }
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

public class InputThresholdComponent : SensorComponent<InputThresholdSensor>
{
    private Dictionary<EntityComponent, int> activatorCounts = new Dictionary<EntityComponent, int>();
    private bool wasOn = false;

    void Update()
    {
        int energy = CalculateEnergy();
        bool isOn = energy >= sensor.threshold;

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

        foreach (var input in sensor.inputs)
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
        foreach (var input in sensor.inputs)
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