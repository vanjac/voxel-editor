using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

// allows properties that reference entities to be serialized
public class EntityReference
{
    [XmlIgnore]
    private WeakReference entityWeakRef;
    public Guid guid;
    public Entity entity
    {
        get
        {
            if (!EntitiesLoaded())
                return null;
            if (guid == Guid.Empty)
                return null;
            else if (entityWeakRef == null) // this happens when the reference is deserialized
            {
                try
                {
                    entityWeakRef = existingEntityIds[guid];
                }
                catch (KeyNotFoundException)
                {
                    guid = Guid.Empty;
                    return null;
                }
            }

            Entity target = (Entity)(entityWeakRef.Target);
            if (target == null || (!target.AliveInEditor() && target.component == null))
            {
                guid = Guid.Empty;
                entityWeakRef = null;
                return null;
            }
            return target;
        }
    }
    public EntityComponent component
    {
        get
        {
            Entity e = entity;
            if (e != null)
                return e.component;
            return null;
        }
    }

    private static Dictionary<Guid, WeakReference> existingEntityIds = new Dictionary<Guid, WeakReference>();
    private static bool entitiesLoaded = false;

    EntityReference() { } // deserialization

    public EntityReference(Entity entity)
    {
        if (entity == null)
        {
            guid = Guid.Empty;
            entityWeakRef = null;
        }
        else
        {
            if (entity.guid == Guid.Empty)
                entity.guid = Guid.NewGuid();
            guid = entity.guid;
            entityWeakRef = new WeakReference(entity);
        }
    }

    public EntityReference(Guid guid)
    {
        this.guid = guid;
    }

    // for comparing property values
    public override bool Equals(object obj)
    {
        var objReference = obj as EntityReference;
        return objReference != null && objReference.entity == entity;
    }

    public override int GetHashCode()
    {
        if (entity == null)
            return 0;
        return entity.GetHashCode();
    }

    public static void ResetEntityIds()
    {
        existingEntityIds.Clear();
        entitiesLoaded = false;
    }

    public static void DoneLoadingEntities()
    {
        entitiesLoaded = true;
    }

    // has the map file finished loading? are EntityReferences safe to be read?
    public static bool EntitiesLoaded()
    {
        return entitiesLoaded;
    }

    public static void AddExistingEntityId(Entity entity, Guid guid)
    {
        if (existingEntityIds.ContainsKey(guid))
        {
            Debug.Log("ERROR: 2 entities have the same GUID! " + guid);
            return;
        }
        entity.guid = guid;
        existingEntityIds[guid] = new WeakReference(entity);
    }
}


public struct Target
{
    public EntityReference entityRef;
    public sbyte direction;

    public Target(Entity entity)
    {
        entityRef = new EntityReference(entity);
        direction = -1;
    }

    public Target(sbyte direction)
    {
        entityRef = new EntityReference(null);
        this.direction = direction;
    }

    public Vector3 DirectionFrom(Vector3 point)
    {
        if (entityRef.entity != null)
        {
            direction = -1; // older versions had default direction as 0
            EntityComponent c = entityRef.component;
            if (c != null)
                return (c.transform.position - point).normalized;
            return Vector3.zero;
        }
        else if (direction == -1)
            return Vector3.zero;
        else
            return Voxel.DirectionForFaceI(direction);
    }

    public bool MatchesDirection(Vector3 point, Vector3 direction)
    {
        Vector3 targetDirection = DirectionFrom(point);
        if (targetDirection == Vector3.zero)
            return true;
        return Vector3.Angle(targetDirection, direction) < 45;
    }

    public override string ToString()
    {
        if (entityRef.entity != null)
            return entityRef.entity.ToString();
        else
            switch (direction)
            {
                case 0:
                    return "West";
                case 1:
                    return "East";
                case 2:
                    return "Down";
                case 3:
                    return "Up";
                case 4:
                    return "South";
                case 5:
                    return "North";
                default:
                    return "None";
            }
    }
}