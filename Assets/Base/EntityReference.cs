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
    public const sbyte WEST = 0;
    public const sbyte EAST = 1;
    public const sbyte DOWN = 2;
    public const sbyte UP = 3;
    public const sbyte SOUTH = 4;
    public const sbyte NORTH = 5;
    public const sbyte NO_DIRECTION = -1;

    public EntityReference entityRef;
    public sbyte direction;

    public Target(Entity entity)
    {
        entityRef = new EntityReference(entity);
        direction = NO_DIRECTION;
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
            direction = NO_DIRECTION; // older versions had default direction as 0
            EntityComponent c = entityRef.component;
            if (c != null)
                return (c.transform.position - point).normalized;
            return Vector3.zero;
        }
        else if (direction == NO_DIRECTION)
            return Vector3.zero;
        else
            return Voxel.DirectionForFaceI(direction);
    }

    public float DistanceFrom(Vector3 point)
    {
        if (entityRef.entity != null)
        {
            EntityComponent c = entityRef.component;
            if (c != null)
                return (c.transform.position - point).magnitude;
            return 0.0f;
        }
        else if (direction == NO_DIRECTION)
            return 0.0f;
        else
            return float.PositiveInfinity;
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
                case WEST:
                    return "West";
                case EAST:
                    return "East";
                case DOWN:
                    return "Down";
                case UP:
                    return "Up";
                case SOUTH:
                    return "South";
                case NORTH:
                    return "North";
                default:
                    return "None";
            }
    }
}