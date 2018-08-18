using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Test", typeof(TestSensor));

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        return gameObject.AddComponent<TestSensorComponent>();
    }
}

public class TestSensorComponent : SensorComponent
{
    public bool newFrame, addActivator, removeActivator, clearActivators;
    public int numActivators, numNewActivators, numNewActivators_next, numRemovedActivators, numRemovedActivators_next;

    public void Update()
    {
        if (newFrame)
        {
            newFrame = false;
            base.LateUpdate();
        }
        if (addActivator)
        {
            addActivator = false;
            AddActivator(GetComponent<EntityComponent>());
        }
        if (removeActivator)
        {
            removeActivator = false;
            RemoveActivator(GetComponent<EntityComponent>());
        }
        if (clearActivators)
        {
            clearActivators = false;
            ClearActivators();
        }
        numActivators = GetActivators().Count;
        numNewActivators = GetNewActivators().Count;
        numNewActivators_next = newActivators_next.Count;
        numRemovedActivators = GetRemovedActivators().Count;
        numRemovedActivators_next = removedActivators_next.Count;
    }

    public override void LateUpdate()
    {
        // do nothing
    }
}