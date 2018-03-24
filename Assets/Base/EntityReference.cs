using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

// allows properties that reference entities to be serialized
public struct EntityReference
{
    [XmlIgnore]
    private Entity _entity;
    public readonly Guid guid;
    public Entity entity
    {
        get
        {
            if (_entity == null && guid != Guid.Empty) // this happens when the reference is deserialized
                _entity = existingEntityIds[guid];
            return _entity;
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

    private static Dictionary<Guid, Entity> existingEntityIds = new Dictionary<Guid,Entity>();
    private static Dictionary<Entity, Guid> entityIds = new Dictionary<Entity,Guid>();

    public EntityReference(Entity entity)
    {
        _entity = entity;
        if (entity == null)
            guid = Guid.Empty;
        else if (!entityIds.ContainsKey(entity))
        {
            guid = Guid.NewGuid();
            entityIds[entity] = guid;
        }
        else
        {
            guid = entityIds[entity];
        }
    }

    public static bool EntityHasId(Entity entity)
    {
        return entityIds.ContainsKey(entity);
    }

    public static void AddExistingEntityId(Entity entity, Guid guid)
    {
        entityIds[entity] = guid;
        existingEntityIds[guid] = entity;
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