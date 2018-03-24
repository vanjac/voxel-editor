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
    private static Dictionary<int, Guid> entityIds = new Dictionary<int, Guid>(); // maps hash code to Guid

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
            int hash = entity.GetHashCode();
            if (!entityIds.ContainsKey(hash))
            {
                guid = Guid.NewGuid();
                entityIds[hash] = guid;
            }
            else
            {
                guid = entityIds[hash];
            }
            entityWeakRef = new WeakReference(entity);
            entity = null;
        }
    }

    public static bool EntityHasId(Entity entity)
    {
        return entityIds.ContainsKey(entity.GetHashCode());
    }

    public static void AddExistingEntityId(Entity entity, Guid guid)
    {
        entityIds[entity.GetHashCode()] = guid;
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
        direction = 0;
    }

    public Target(sbyte direction)
    {
        entityRef = new EntityReference(null);
        this.direction = direction;
    }

    public Vector3 directionFrom(Vector3 point)
    {
        if (entityRef.entity == null)
            return Voxel.DirectionForFaceI(direction);
        else
        {
            EntityComponent c = entityRef.component;
            if(c != null)
                return (c.transform.position - point).normalized;
            return Vector3.zero;
        }
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
                    return "";
            }
    }
}