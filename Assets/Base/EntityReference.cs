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
            if (_entity == null) // this happens when the reference is deserialized
                _entity = existingEntityIds[guid];
            return _entity;
        }
    }

    private static Dictionary<Guid, Entity> existingEntityIds = new Dictionary<Guid,Entity>();
    private static Dictionary<Entity, Guid> entityIds = new Dictionary<Entity,Guid>();

    public EntityReference(Entity entity)
    {
        _entity = entity;
        if (!entityIds.ContainsKey(entity))
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