using System;
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
    public const sbyte RANDOM = 6;
    public const sbyte LOCAL_BIT = 8;
    public const sbyte NO_DIRECTION = -1;

    public EntityReference entityRef;
    public sbyte direction;

    [XmlIgnore]
    private Vector3 randomDirection;

    public Target(Entity entity)
    {
        entityRef = new EntityReference(entity);
        direction = NO_DIRECTION;
        randomDirection = Vector3.zero;
    }

    public Target(sbyte direction)
    {
        entityRef = new EntityReference(null);
        this.direction = direction;
        randomDirection = Vector3.zero;
    }

    public void PickRandom()
    {
        if (direction != RANDOM)
            return;
        float angle = UnityEngine.Random.Range(0.0f, 2 * Mathf.PI);
        randomDirection = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle));
    }

    public Vector3 DirectionFrom(Transform transform)
    {
        if (entityRef.entity != null)
        {
            direction = NO_DIRECTION; // older versions had default direction as 0
            EntityComponent c = entityRef.component;
            if (c != null)
                return (c.transform.position - transform.position).normalized;
            return Vector3.zero;
        }
        else if (direction == NO_DIRECTION)
            return Vector3.zero;
        else if (direction == RANDOM)
            return randomDirection;
        else if ((direction & LOCAL_BIT) != 0)
            return transform.TransformDirection(Voxel.DirectionForFaceI(direction & ~LOCAL_BIT));
        else
            return Voxel.DirectionForFaceI(direction);
    }

    public float DistanceFrom(Transform transform)
    {
        if (entityRef.entity != null)
        {
            EntityComponent c = entityRef.component;
            if (c != null)
                return (c.transform.position - transform.position).magnitude;
            return 0.0f;
        }
        else if (direction == NO_DIRECTION)
            return 0.0f;
        else
            return float.PositiveInfinity;
    }

    public bool MatchesDirection(Transform transform, Vector3 direction)
    {
        Vector3 targetDirection = DirectionFrom(transform);
        if (targetDirection == Vector3.zero)
            return true;
        return Vector3.Angle(targetDirection, direction) < 45;
    }

    public override string ToString()
    {
        if (entityRef.entity != null)
            return entityRef.entity.ToString();
        else
        {
            string dirStr = "None";
            switch (direction & ~LOCAL_BIT)
            {
                case WEST:
                    dirStr = "West";
                    break;
                case EAST:
                    dirStr = "East";
                    break;
                case DOWN:
                    dirStr = "Down";
                    break;
                case UP:
                    dirStr = "Up";
                    break;
                case SOUTH:
                    dirStr = "South";
                    break;
                case NORTH:
                    dirStr = "North";
                    break;
                case RANDOM:
                    dirStr = "Random";
                    break;
            }
            if ((direction & LOCAL_BIT) != 0 && direction != NO_DIRECTION)
                return "Local " + dirStr;
            else
                return dirStr;
        }
    }
}